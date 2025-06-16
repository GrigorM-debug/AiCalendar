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
        private readonly IEventService _eventService;
        private readonly IRepository<Event> _eventRepository;

        public EventService(IEventService eventService, IRepository<Event> eventRepository)
        {
            _eventService = eventService;
            _eventRepository = eventRepository;
        }

        public async Task<EventDto?> GetEventByIdAsync(Guid id)
        {
            Event? e = await _eventRepository
                .WithIncludes(e => e.Participants, e => e.Participants.Select(p => p.User))
                .FirstOrDefaultAsync(e => e.Id == id);

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

        public async Task<bool> EventExistsByIdAsync(Guid id)
        {
            bool isEventExisting = await _eventRepository.ExistsByIdAsync(id);

            return isEventExisting;
        }

        public async Task<bool> HasOverlappingEvents(Guid userId, DateTime startTime, DateTime endTime)
        {
            var hasOverlappingEvents = await _eventRepository.ExistsByExpressionAsync(e =>
                e.IsCancelled == false &&
                (startTime >= e.StartTime && startTime < e.EndTime) ||
                (endTime > e.StartTime && endTime <= e.EndTime) ||
                (startTime <= e.StartTime && endTime >= e.EndTime));

            return hasOverlappingEvents;
        }

        public async Task<bool> IsUserEventCreator(Guid eventId, Guid userId)
        {
            bool isCreator = await _eventRepository.ExistsByExpressionAsync(e => e.Id == eventId && e.CreatorId == userId);

            return isCreator;
        }

        public async Task DeleteEventAsync(Guid eventId)
        {
            await _eventRepository.DeleteAsync(eventId);
            await _eventRepository.SaveChangesAsync();
        }

        public async Task<EventDto> UpdateEvent(Guid eventId, UpdateEventDto updateEventDto, Guid userId)
        {
            Event? eventToUpdate =
                await _eventRepository
                    .WithIncludes(e => e.Participants, e => e.Participants.Select(p => p.User))
                    .FirstOrDefaultAsync(e => e.Id == eventId && e.CreatorId == userId);

            eventToUpdate.Title = updateEventDto.Title;
            eventToUpdate.Description = updateEventDto.Description;
            eventToUpdate.StartTime = updateEventDto.StartTime;
            eventToUpdate.EndTime = updateEventDto.EndTime;

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

        public async Task<EventDto> CancelEventAsync(Guid eventId, Guid userId)
        {
            Event? eventToCancel = await _eventRepository
                .WithIncludes(e => e.Participants, e => e.Participants.Select(p => p.User))
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
    }
}
