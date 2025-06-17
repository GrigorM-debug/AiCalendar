using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Services.Events
{
    public class EventService : IEventService
    {
        private readonly IRepository<Event> _eventRepository;

        public EventService(IRepository<Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        /// <summary>
        /// Retrieves an event by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event matching the specified identifier, or <c>null</c> if not found.</returns>
        public async Task<EventDto?> GetEventByIdAsync(Guid id)
        {
           IQueryable<Event> query = _eventRepository
                .WithIncludes(e => e.Participants)
                .AsQueryable();

           query = query
               .Include(e => e.Participants)
               .ThenInclude(p => p.User);
            
           Event? e = await query
                .FirstOrDefaultAsync(e => e.Id == id && e.IsCancelled == false);

            if (e == null)
            {
                return null;
            }

            EventDto eventDto = new EventDto
            {
                Id = e.Id.ToString(),
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartTime,
                EndDate = e.EndTime,
                CreatorId = e.CreatorId.ToString(),
                IsCancelled = e.IsCancelled,
                Participants = e.Participants.Select(p => new UserDto()
                {
                    Id = p.UserId.ToString(),
                    UserName = p.User.UserName,
                    Email = p.User.Email
                }).ToList()
            };

            return eventDto;
        }

        /// <summary>
        /// Creates a new event with the specified details and creator.
        /// </summary>
        /// <param name="createEventDto">The details of the event to create.</param>
        /// <param name="creatorId">The unique identifier of the user creating the event.</param>
        /// <returns>The created event.</returns>
        public async Task<EventDto> CreateEventAsync(CreateEventDto createEventDto, Guid creatorId)
        {
            Event newEvent = new Event
            {
                Title = createEventDto.Title,
                Description = createEventDto.Description,
                StartTime = createEventDto.StartTime,
                EndTime = createEventDto.EndTime,
                CreatorId = creatorId,
                IsCancelled = false
            };

            await _eventRepository.AddAsync(newEvent);
            await _eventRepository.SaveChangesAsync();

            EventDto eventDto = new EventDto
            {
                Id = newEvent.Id.ToString(),
                Title = newEvent.Title,
                Description = newEvent.Description,
                StartDate = newEvent.StartTime,
                EndDate = newEvent.EndTime,
                CreatorId = newEvent.CreatorId.ToString(),
                IsCancelled = newEvent.IsCancelled
            };

            return eventDto;
        }

        /// <summary>
        /// Determines whether an event with the specified unique identifier exists.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns><c>true</c> if the event exists; otherwise, <c>false</c>.</returns>
        public async Task<bool> EventExistsByIdAsync(Guid id)
        {
            bool isEventExisting = await _eventRepository.ExistsByIdAsync(id);

            return isEventExisting;
        }

        /// <summary>
        /// Determines whether the specified user has any events that overlap with the given time range.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="startTime">The start time of the range to check for overlaps.</param>
        /// <param name="endTime">The end time of the range to check for overlaps.</param>
        /// <returns><c>true</c> if there are overlapping events for the user; otherwise, <c>false</c>.</returns>
        public async Task<bool> HasOverlappingEvents(Guid userId, DateTime startTime, DateTime endTime)
        {
            var hasOverlappingEvents = await _eventRepository.ExistsByExpressionAsync(e =>
                e.IsCancelled == false &&
                (startTime >= e.StartTime && startTime < e.EndTime) ||
                (endTime > e.StartTime && endTime <= e.EndTime) ||
                (startTime <= e.StartTime && endTime >= e.EndTime));

            return hasOverlappingEvents;
        }

        /// <summary>
        /// Determines whether the specified user is the creator of the given event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns><c>true</c> if the user is the creator of the event; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsUserEventCreator(Guid eventId, Guid userId)
        {
            bool isCreator = await _eventRepository.ExistsByExpressionAsync(e => e.Id == eventId && e.CreatorId == userId);

            return isCreator;
        }

        /// <summary>
        /// Deletes the event with the specified unique identifier.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeleteEventAsync(Guid eventId)
        {
            await _eventRepository.DeleteAsync(eventId);
            await _eventRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the details of an existing event with the specified identifier.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to update.</param>
        /// <param name="updateEventDto">The updated event details.</param>
        /// <param name="userId">The unique identifier of the user performing the update.</param>
        /// <returns>The updated event.</returns>
        public async Task<EventDto> UpdateEvent(Guid eventId, UpdateEventDto updateEventDto, Guid userId)
        {
            IQueryable<Event> query = _eventRepository
                .WithIncludes(e => e.Creator)
                .AsQueryable();

            query = query
                .Include(e => e.Participants)
                .ThenInclude(p => p.User);

            Event? eventToUpdate =
                await query
                    .FirstOrDefaultAsync(e => e.Id == eventId && e.CreatorId == userId);

            if (!string.IsNullOrEmpty(updateEventDto.Title))
            {
                eventToUpdate.Title = updateEventDto.Title;
            }

            if (!string.IsNullOrEmpty(updateEventDto.Description))
            {
                eventToUpdate.Description = updateEventDto.Description;
            }

            if (updateEventDto.StartTime.HasValue)
            {
                eventToUpdate.StartTime = updateEventDto.StartTime.Value;
            }

            if (updateEventDto.EndTime.HasValue)
            {
                eventToUpdate.EndTime = updateEventDto.EndTime.Value;
            }


            _eventRepository.UpdateAsync(eventToUpdate);
            await _eventRepository.SaveChangesAsync();

            EventDto updatedEventDto = new EventDto
            {
                Id = eventToUpdate.Id.ToString(),
                Title = eventToUpdate.Title,
                Description = eventToUpdate.Description,
                StartDate = eventToUpdate.StartTime,
                EndDate = eventToUpdate.EndTime,
                CreatorId = eventToUpdate.CreatorId.ToString(),
                IsCancelled = eventToUpdate.IsCancelled,
                Participants = eventToUpdate.Participants.Select(p => new UserDto
                {
                    Id = p.UserId.ToString(),
                    UserName = p.User.UserName,
                    Email = p.User.Email
                }).ToList()
            };

            return updatedEventDto;
        }

        /// <summary>
        /// Determines whether the specified user has any events that overlap with the given time range, excluding the event with the specified identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="startTime">The start time of the range to check for overlaps.</param>
        /// <param name="endTime">The end time of the range to check for overlaps.</param>
        /// <param name="eventId">The unique identifier of the event to exclude from the overlap check.</param>
        /// <returns><c>true</c> if there are overlapping events for the user (excluding the specified event); otherwise, <c>false</c>.</returns>
        public async Task<bool> HasOverlappingEventsExcludingTheCurrentEvent(Guid userId, DateTime startTime,
            DateTime endTime, Guid eventId)
        {
            bool hasOverlappingEvents = await _eventRepository
                .ExistsByExpressionAsync(e =>
                    e.Id != eventId &&
                    e.CreatorId == userId &&
                    !e.IsCancelled &&
                    ((startTime >= e.StartTime && startTime < e.EndTime) ||
                     (endTime > e.StartTime && endTime <= e.EndTime) ||
                     (startTime <= e.StartTime && endTime >= e.EndTime)));

            return hasOverlappingEvents;
        }

        /// <summary>
        /// Cancels the event with the specified unique identifier on behalf of the specified user.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to cancel.</param>
        /// <param name="userId">The unique identifier of the user performing the cancellation.</param>
        /// <returns>The updated event with its cancellation status set.</returns>
        public async Task<EventDto> CancelEventAsync(Guid eventId, Guid userId)
        {
            IQueryable<Event> query = _eventRepository
                .WithIncludes(e => e.Creator)
                .AsQueryable();

            query = query
                .Include(e => e.Participants)
                .ThenInclude(p => p.User);

            Event? eventToCancel = await query
                .FirstOrDefaultAsync(e => e.Id == eventId && e.CreatorId == userId);

            eventToCancel.IsCancelled = true;

            _eventRepository.UpdateAsync(eventToCancel);
            await _eventRepository.SaveChangesAsync();

            EventDto cancelledEventDto = new EventDto
            {
                Id = eventToCancel.Id.ToString(),
                Title = eventToCancel.Title,
                Description = eventToCancel.Description,
                StartDate = eventToCancel.StartTime,
                EndDate = eventToCancel.EndTime,
                CreatorId = eventToCancel.CreatorId.ToString(),
                IsCancelled = eventToCancel.IsCancelled,
                Participants = eventToCancel.Participants.Select(p => new UserDto
                {
                    Id = p.UserId.ToString(),
                    UserName = p.User.UserName,
                    Email = p.User.Email
                }).ToList()
            };

            return cancelledEventDto;
        }

        /// <summary>
        /// Retrieves events based on specified filter criteria
        /// </summary>
        /// <param name="filter">Optional filter criteria for events</param>
        /// <returns>A collection of filtered events</returns>
        public async Task<IEnumerable<EventDto>> GetEventsAsync(EventFilterCriteriaDto? filter = null)
        {
            IQueryable<Event> query = _eventRepository
                .WithIncludes(e => e.Creator) 
                .AsQueryable();

            query = query
                .Include(e => e.Participants)          
                .ThenInclude(p => p.User);

            if (filter != null)
            {
                if (filter.StartDate.HasValue)
                {
                    query = query.Where(e => e.StartTime >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(e => e.EndTime <= filter.EndDate.Value);
                }

                if (filter.IsCancelled.HasValue)
                {
                    query = query.Where(e => e.IsCancelled == filter.IsCancelled.Value);
                }
            }

            IEnumerable<EventDto> events = await query
                .Select(e => new EventDto
                {
                    Id = e.Id.ToString(),
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartTime,
                    EndDate = e.EndTime,
                    CreatorId = e.CreatorId.ToString(),
                    IsCancelled = e.IsCancelled,
                    Participants = e.Participants.Select(p => new UserDto
                    {
                        Id = p.UserId.ToString(),
                        UserName = p.User.UserName,
                        Email = p.User.Email
                    }).ToList()
                })
                .ToListAsync();

            return events;
        }

    }
}
