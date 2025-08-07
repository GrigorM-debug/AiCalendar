using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.Models;

namespace AiCalendar.WebApi.Services.Events.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Retrieves an event by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns>The event matching the specified identifier, or <c>null</c> if not found.</returns>
        Task<EventDto?> GetEventByIdAsync(Guid id);

        /// <summary>
        /// Creates a new event with the specified details and creator.
        /// </summary>
        /// <param name="createEventDto">The details of the event to create.</param>
        /// <param name="creatorId">The unique identifier of the user creating the event.</param>
        /// <returns>The created event.</returns>
        Task<EventDto> CreateEventAsync(CreateEventDto createEventDto, Guid creatorId);

        /// <summary>
        /// Determines whether an event with the specified unique identifier exists.
        /// </summary>
        /// <param name="id">The unique identifier of the event.</param>
        /// <returns><c>true</c> if the event exists; otherwise, <c>false</c>.</returns>
        Task<bool> EventExistsByIdAsync(Guid id);

        /// <summary>
        /// Determines whether the specified user has any events that overlap with the given time range.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="startTime">The start time of the range to check for overlaps.</param>
        /// <param name="endTime">The end time of the range to check for overlaps.</param>
        /// <returns><c>true</c> if there are overlapping events for the user; otherwise, <c>false</c>.</returns>
        Task<bool> HasOverlappingEvents(Guid userId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Determines whether the specified user is the creator of the given event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns><c>true</c> if the user is the creator of the event; otherwise, <c>false</c>.</returns>
        Task<bool> IsUserEventCreator(Guid eventId, Guid userId);

        /// <summary>
        /// Deletes the event with the specified unique identifier.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteEventAsync(Guid eventId);

        /// <summary>
        /// Updates the details of an existing event with the specified identifier.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to update.</param>
        /// <param name="updateEventDto">The updated event details.</param>
        /// <param name="userId">The unique identifier of the user performing the update.</param>
        /// <returns>The updated event.</returns>
        Task<EventDto> UpdateEvent(Guid eventId, UpdateEventDto updateEventDto, Guid userId);

        /// <summary>
        /// Determines whether the specified user has any events that overlap with the given time range, excluding the event with the specified identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="startTime">The start time of the range to check for overlaps.</param>
        /// <param name="endTime">The end time of the range to check for overlaps.</param>
        /// <param name="eventId">The unique identifier of the event to exclude from the overlap check.</param>
        /// <returns><c>true</c> if there are overlapping events for the user (excluding the specified event); otherwise, <c>false</c>.</returns>
        Task<bool> HasOverlappingEventsExcludingTheCurrentEvent(Guid userId, DateTime startTime, DateTime endTime, Guid eventId);

        /// <summary>
        /// Cancels the event with the specified unique identifier on behalf of the specified user.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to cancel.</param>
        /// <param name="userId">The unique identifier of the user performing the cancellation.</param>
        /// <returns>The updated event with its cancellation status set.</returns>
        Task<EventDto> CancelEventAsync(Guid eventId, Guid userId);

        /// <summary>
        /// Retrieves events based on specified filter criteria
        /// </summary>
        /// <param name="filter">Optional filter criteria for events</param>
        /// <returns>A collection of filtered events</returns>
        Task<IEnumerable<EventDto>> GetEventsAsync(EventFilterCriteriaDto? filter = null);


        /// <summary>
        /// Checks if an event with the specified unique identifier is already cancelled.
        /// <param name="eventId">The id of the event</param>
        /// <returns>Boolean depending on event cancelling condition</returns>
        /// </summary>
        Task<bool> CheckIfEventIsAlreadyCancelled(Guid eventId);
    }
}
