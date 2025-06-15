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
        Task<User> RegisterAsync(LoginAndRegisterInputDto input);
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
    }
}
