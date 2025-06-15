using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users.Interfaces;

namespace AiCalendar.WebApi.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRepository<Event> _eventRepository;

        public UserService(IRepository<User> userRepository, IPasswordHasher passwordHasher, IRepository<Event> eventRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _eventRepository = eventRepository;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="input">The registration input data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<UserDto> RegisterAsync(LoginAndRegisterInputDto input)
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

            UserDto userDto = new UserDto
            {
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Email = user.Email
            };

            return userDto;
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

        /// <summary>
        /// Retrieves a user by username and email, if they exist.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <param name="email">The email to search for.</param>
        /// <returns>A task representing the asynchronous operation, with the user if found; otherwise, null.</returns>
        public async Task<User?> GetUserByUserNameAndEmail(string username, string email)
        {
            User? user = await _userRepository.GetByExpressionAsync(u => u.UserName == username && u.Email == email);

            if (user == null)
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Delete user by userId, if they exist.
        /// </summary>
        /// <param name="userId">The user's id</param>
        public async Task DeleteUserAsync(Guid userId)
        {
            await _userRepository.DeleteAsync(userId);
        }

        /// <summary>
        /// Updates a user by their userId, if the user exists.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="updateUserDto">The updated user data.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the updated user if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
        {
            User? user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                user.Email = updateUserDto.Email;
            }

            if(!string.IsNullOrEmpty(updateUserDto.UserName))
            {
                user.UserName = updateUserDto.UserName;
            }

            if(!string.IsNullOrEmpty(updateUserDto.OldPassword) && !string.IsNullOrEmpty(updateUserDto.NewPassword))
            {
                //Check if old password is correct
                string oldPasswordHash = _passwordHasher.HashPassword(updateUserDto.OldPassword);
                if (user.PasswordHashed != oldPasswordHash)
                {
                    throw new Exception("Old password is incorrect.");
                }

                string newPasswordHash = _passwordHasher.HashPassword(updateUserDto.NewPassword);
                //Check if new password is same as the old password
                if (user.PasswordHashed == newPasswordHash)
                {
                    throw new Exception("New password cannot be the same as the old password.");
                }

                user.PasswordHashed = newPasswordHash;
            }

            await _userRepository.SaveChangesAsync();

            UserDto userDto = new UserDto
            {
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Email = user.Email
            };

            return userDto;
        }

        /// <summary>
        /// Asynchronously retrieves a collection of events created by the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose created events are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="EventDto"/> objects created by the user.</returns>
        public async Task<IEnumerable<EventDto>> GetUserCreatedEventsAsync(Guid userId)
        {
            //Check this
            IEnumerable<Event> userCreatedEvents = await _eventRepository.GetAllByExpressionAsync(e => e.CreatorId == userId);

            IEnumerable<EventDto> userCreatedEventsDtos = userCreatedEvents.Select(e => new EventDto
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
                    Email = p.User.Email,
                    UserName = p.User.UserName
                }).ToList()
            });

            return userCreatedEventsDtos;
        }
    }
}
