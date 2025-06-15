using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;

namespace AiCalendar.WebApi.Services.Users.Interfaces
{
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
        /// Delete user by userId, if they exist.
        /// </summary>
        /// <param name="userId">The user's id</param>
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
        /// Asynchronously retrieves a collection of events created by the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose created events are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="EventDto"/> objects created by the user.</returns>
        Task<IEnumerable<EventDto>> GetUserCreatedEventsAsync(Guid userId);
    }
}
