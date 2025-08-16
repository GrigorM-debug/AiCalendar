using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;

namespace AiCalendar.WebApi.Services.Users.Interfaces
{
    /// <summary>
    /// Service for managing user-related operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="input">The registration input data.</param>
        /// <returns>A task representing the asynchronous operation, with the user</returns>
        Task<UserDto> RegisterAsync(LoginAndRegisterInputDto input);
        /// <summary>
        /// Checks if a user exists by username.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence.</returns>
        Task<bool> UserExistsByUsernameAsync(string username);

        /// <summary>
        /// Retrieves a user by username, if they exist.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>A task representing the asynchronous operation, with the user if found; otherwise, null.</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Retrieves a user by userId, if they exist.
        /// </summary>
        /// <param name="userId">The userId to search for.</param>
        /// <returns>A task representing the asynchronous operation, with the user if found; otherwise, null.</returns>
        Task<User?> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Checks if user by userId, if they exist.
        /// </summary>
        /// <param name="userId">The userId to search for.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence</returns>
        Task<bool> UserExistsByIdAsync(Guid userId);

        /// <summary>
        /// Retrieves a user by username and email, if they exist.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <param name="email">The email to search for.</param>
        /// <returns>A task representing the asynchronous operation, with the user if found; otherwise, null.</returns>
        Task<User?> GetUserByUserNameAndEmail(string username, string email);

        /// <summary>
        /// Deletes a user and all their associated participant records
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete</param>
        /// <returns>True if the user was successfully deleted</returns>
        Task DeleteUserAsync(Guid userId);

        /// <summary>
        /// Updates a user by their userId, if the user exists.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="updateUserDto">The updated user data.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the updated user if found; otherwise, <c>null</c>.
        /// </returns>
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);

        /// <summary>
        /// Retrieves all events where the specified user is a participant
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>A collection of events where the user is a participant</returns>
        Task<IEnumerable<EventDto>> GetUserParticipatingEventsAsync(Guid userId);

        /// <summary>
        /// Check if a user has any active events.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Returns true if user has any active events or false if user hasn't</returns>
        Task<bool> CheckIfUserHasActiveEvents(Guid userId);

        /// <summary>
        /// Retrieves events created by the specified user with optional filtering.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="filter">Optional filter criteria for the events</param>
        /// <returns>A collection of <see cref="EventDto"/> matching the specified criteria</returns>
        public Task<IEnumerable<EventDto>> GetUserEventsAsync(Guid userId, EventFilterCriteriaDto? filter = null);

        /// <summary>
        /// Retrieves users based on specified filter criteria
        /// </summary>
        /// <param name="filter">Optional filter criteria for users</param>
        /// <returns>A collection of filtered users</returns>
        Task<IEnumerable<UserDtoExtended>> GetUsersAsync(UserFilterCriteriaDto? filter = null);

        /// <summary>
        /// Checks if a user exists by username and email.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <param name="email">The email to search for.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence.</returns>
        Task<bool> UserExistsByUsernameAndEmailAsync(string? username, string? email);
    }
}
