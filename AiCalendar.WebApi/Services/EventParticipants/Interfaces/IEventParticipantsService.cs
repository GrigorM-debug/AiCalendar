using AiCalendar.WebApi.DTOs.Users;

namespace AiCalendar.WebApi.Services.EventParticipants.Interfaces
{
    public interface IEventParticipantsService
    {
        /// <summary>
        /// Retrieves all participants for the specified event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>A collection of users participating in the event.</returns>
        Task<IEnumerable<UserDto>> GetParticipantsByEventIdAsync(Guid eventId);

        /// <summary>
        /// Determines whether the specified user is a participant in the given event.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns><c>true</c> if the user is a participant in the event; otherwise, <c>false</c>.</returns>
        Task<bool> IsUserEventParticipant(Guid userId, Guid eventId);

        /// <summary>
        /// Adds a user as a participant to the specified event.
        /// </summary>
        /// <param name="participantId">The unique identifier of the user.</param>
        /// <param name="eventId">The unique identifier of the event.</param>
        Task AddParticipantAsync(Guid participantId, Guid eventId);

        /// <summary>
        /// Removes a user as a participant from the specified event.
        /// </summary>
        /// <param name="participantId">The unique identifier of the user.</param>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveParticipantAsync(Guid participantId, Guid eventId);
    }
}
