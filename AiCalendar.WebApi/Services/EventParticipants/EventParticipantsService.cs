using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.EventParticipants.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Services.EventParticipants
{
    public class EventParticipantsService : IEventParticipantsService
    {
        private readonly IRepository<Participant> _participantRepository;

        public EventParticipantsService(IRepository<Participant> participantRepository)
        {
            _participantRepository = participantRepository;
        }

        /// <summary>
        /// Retrieves all participants for the specified event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>A collection of users participating in the event.</returns>
        public async Task<IEnumerable<UserDto>> GetParticipantsByEventIdAsync(Guid eventId)
        {
            IEnumerable<UserDto> participants = await _participantRepository
                .WithIncludes(p => p.User)
                .Select(p => new UserDto()
                {
                    Id = p.UserId.ToString(),
                    UserName = p.User.UserName,
                    Email = p.User.Email,
                })
                .ToListAsync();

            return participants;
        }

        /// <summary>
        /// Determines whether the specified user is a participant in the given event.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns><c>true</c> if the user is a participant in the event; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsUserEventParticipant(Guid userId, Guid eventId)
        {
            bool isParticipant = await _participantRepository
                .ExistsByExpressionAsync(p => p.UserId == userId && p.EventId == eventId);

            return isParticipant;
        }

        /// <summary>
        /// Adds a user as a participant to the specified event.
        /// </summary>
        /// <param name="participantId">The unique identifier of the user.</param>
        /// <param name="eventId">The unique identifier of the event.</param>
        public async Task AddParticipantAsync(Guid participantId, Guid eventId)
        {
            Participant participant = new Participant
            {
                UserId = participantId,
                EventId = eventId
            };

            await _participantRepository.AddAsync(participant);
            await _participantRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a user as a participant from the specified event.
        /// </summary>
        /// <param name="participantId">The unique identifier of the user.</param>
        /// <param name="eventId">The unique identifier of the event.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveParticipantAsync(Guid participantId, Guid eventId)
        {
            Participant? participant = await _participantRepository
                .GetByExpressionAsync(p => p.UserId == participantId && p.EventId == eventId);

            if (participant != null)
            {
                _participantRepository.DeleteAsync(participantId);
                await _participantRepository.SaveChangesAsync();
            }
        }
    }
}
