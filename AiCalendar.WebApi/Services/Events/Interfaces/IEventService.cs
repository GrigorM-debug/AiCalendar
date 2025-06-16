using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.Models;

namespace AiCalendar.WebApi.Services.Events.Interfaces
{
    public interface IEventService
    {
        Task<EventDto?> GetEventByIdAsync(Guid id);
        Task<EventDto> CreateEventAsync(CreateEventDto createEventDto, Guid creatorId);

        Task<bool> EventExistsByIdAsync(Guid id);

        Task<bool> HasOverlappingEvents(Guid userId, DateTime startTime, DateTime endTime);

        Task<bool> IsUserEventCreator(Guid eventId, Guid userId);

        Task DeleteEventAsync(Guid eventId);

        Task<EventDto> UpdateEvent(Guid eventId, UpdateEventDto updateEventDto, Guid userId);

        Task<bool> HasOverlappingEventsExcludingTheCurrentEvent(Guid userId, DateTime startTime, DateTime endTime, Guid eventId);

        Task<EventDto> CancelEventAsync(Guid eventId, Guid userId);
    }
}
