using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users.Interfaces;

namespace AiCalendar.WebApi.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IRepository<User> userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="input">The registration input data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<User> RegisterAsync(LoginAndRegisterInputDto input)
        {
            string hashedPassword = _passwordHasher.HashPassword(input.Password);

            User user = new User
            {
                UserName = input.UserName,
                Email = input.Email,
                PasswordHashed = hashedPassword,
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Checks if a user exists by username.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence.</returns>
        public async Task<bool> UserExistsByUsernameAsync(string username)
        {
            return await _userRepository.ExistsByExpressionAsync(u => u.UserName == username);
        }

        /// <summary>
        /// Retrieves a user by username, if they exist.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>A task representing the asynchronous operation, with the user if found; otherwise, null.</returns>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            User? user = await _userRepository.GetByExpressionAsync(u => u.UserName == username);

            if(user == null)
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Retrieves a user by userId, if they exist.
        /// </summary>
        /// <param name="userId">The userId to search for.</param>
        /// <returns>A task representing the asynchronous operation, with the user if found; otherwise, null.</returns>
        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            User? user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Checks if user by userId, if they exist.
        /// </summary>
        /// <param name="userId">The userId to search for.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean indicating existence</returns>
        public async Task<bool> UserExistsByIdAsync(Guid userId)
        {
            return await _userRepository.ExistsByIdAsync(userId);
        }
    }
}
