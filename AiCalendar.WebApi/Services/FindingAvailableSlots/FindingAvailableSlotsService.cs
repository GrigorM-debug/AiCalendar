using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.FindingAvailableSlots;
using AiCalendar.WebApi.Models;

namespace AiCalendar.WebApi.Services.FindingAvailableSlots
{
    public class FindingAvailableSlotsService : IFindingAvailableSlotsService
    {
        private readonly IRepository<Event> _eventRepository;

        public FindingAvailableSlotsService(IRepository<Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        /// <summary>
        /// Asynchronously finds available slots based on the specified search criteria.
        /// </summary>
        /// <param name="request">The request object containing search parameters for available slots.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains collection of <see cref="AvailableSlotsDto"/> with the available slots data.</returns>
        public async Task<List<AvailableSlotsDto>> FindAvailableSlotsAsync(FindingAvailableSlotsDto request)
        {
            if (!request.ParticipantsIds.Any())
            {
                return new List<AvailableSlotsDto>
                {
                    new AvailableSlotsDto
                    {
                        StartTime = request.SearchStartDateTime,
                        EndTime = request.SearchEndDateTime
                    }
                };
            }

            var allParticipantsEvents = await _eventRepository.GetAllByExpressionAsync(e => e.Participants.Any(p => request.ParticipantsIds.Contains(p.UserId.ToString())));

            var filteredEvents = allParticipantsEvents
                .Where(e => e.StartTime < request.SearchEndDateTime && e.EndTime > request.SearchStartDateTime)
                .ToList();

            var allBusyIntervals = new List<(DateTime Start, DateTime End)>();

            foreach (var participantId in request.ParticipantsIds)
            {
                var participantEvents = filteredEvents
                    .Where(e => e.Participants.Any(p => p.UserId.ToString() == participantId))
                    .Select(e => (e.StartTime, e.EndTime))
                    .ToList();

                allBusyIntervals.AddRange(participantEvents);
            }

            allBusyIntervals = allBusyIntervals
                .Distinct()
                .OrderBy(interval => interval.Start)
                .ToList();

            var mergeTotalBusy = new List<(DateTime Start, DateTime End)>();

            if(allBusyIntervals.Count > 0)
            {
                var currentMergeStart = allBusyIntervals[0].Start;
                var currentMergeEnd = allBusyIntervals[0].End;

                foreach (var (start, end) in allBusyIntervals.Skip(1))
                {
                    if (start <= currentMergeStart)
                    {
                        currentMergeEnd = Max(currentMergeEnd, end);
                    }
                    else
                    {
                        mergeTotalBusy.Add((currentMergeStart, currentMergeEnd));
                        currentMergeStart = start;
                        currentMergeEnd = end;
                    }
                }
                mergeTotalBusy.Add((currentMergeStart, currentMergeEnd));
            }

            var availableSlots = new List<AvailableSlotsDto>();
            var desiredTimeSpan = TimeSpan.FromMinutes(request.SlotDurationInMinutes);
            var currentSearchPoint = request.SearchStartDateTime;

            foreach (var (busyStart, busyEnd) in mergeTotalBusy)
            {
                var potentialFreeStart = Max(currentSearchPoint, request.SearchStartDateTime);

                if (potentialFreeStart < busyStart)
                {
                    var potentialFreeEnd = Min(busyStart, request.SearchEndDateTime);
                    if (potentialFreeEnd - potentialFreeStart >= desiredTimeSpan)
                    {
                        availableSlots.Add(new AvailableSlotsDto()
                        {
                            StartTime = potentialFreeStart,
                            EndTime = potentialFreeEnd
                        });
                        if (availableSlots.Count >= request.NumberOfSlotsToFind)
                        {
                            return availableSlots; 
                        }
                    }
                }

                currentSearchPoint = Max(currentSearchPoint, busyEnd);
            }

            var finalPotentialFreeStart = Max(currentSearchPoint, request.SearchStartDateTime);

            if (finalPotentialFreeStart < request.SearchEndDateTime)
            {
                if (request.SearchEndDateTime - finalPotentialFreeStart >= desiredTimeSpan)
                {
                    availableSlots.Add(new AvailableSlotsDto()
                    {
                        StartTime = finalPotentialFreeStart,
                        EndTime = request.SearchEndDateTime
                    });
                }
            }

            return availableSlots.Take(request.NumberOfSlotsToFind).ToList();
        }

        private DateTime Max(DateTime d1, DateTime d2) => d1 > d2 ? d1 : d2;
        private DateTime Min(DateTime d1, DateTime d2) => d1 < d2 ? d1 : d2;
    }
}
