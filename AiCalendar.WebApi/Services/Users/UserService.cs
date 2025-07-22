using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Services.Users
{
    /// <summary>
    /// Service for managing user-related operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Participant> _participantRepository;

        public UserService(IRepository<User> userRepository, IPasswordHasher passwordHasher, IRepository<Event> eventRepository, IRepository<Participant> participantRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _eventRepository = eventRepository;
            _participantRepository = participantRepository;
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
        /// Deletes a user and all their associated participant records
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete</param>
        /// <returns>True if the user was successfully deleted</returns>
        public async Task DeleteUserAsync(Guid userId)
        {
            //User? user = await _userRepository.GetByIdAsync(userId);

            // Delete all participations of the user
            IEnumerable<Participant> participations = await _userRepository
                .WithIncludes(u => u.Participations)
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Participations)
                .ToListAsync();

            _participantRepository.RemoveRange(participations);
            await _participantRepository.SaveChangesAsync();

            // Delete the user
            await _userRepository.DeleteAsync(userId);
            await _userRepository.SaveChangesAsync();
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

            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                user.Email = updateUserDto.Email;
            }

            if (!string.IsNullOrEmpty(updateUserDto.UserName))
            {
                user.UserName = updateUserDto.UserName;
            }

            //Check if old password is correct
            if (!string.IsNullOrEmpty(updateUserDto.OldPassword) && !string.IsNullOrEmpty(updateUserDto.NewPassword))
            {
                bool isOldPasswordValid =
                    _passwordHasher.VerifyPassword(user.PasswordHashed, updateUserDto.OldPassword);

                if (!isOldPasswordValid)
                {
                    throw new Exception("Old password is incorrect.");
                }

                //Check if new password is same as the old password
                if (updateUserDto.OldPassword == updateUserDto.NewPassword)
                {
                    throw new Exception("New password cannot be the same as the old password.");
                }

                string newPasswordHash = _passwordHasher.HashPassword(updateUserDto.NewPassword);

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
        /// Retrieves all events where the specified user is a participant
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>A collection of events where the user is a participant</returns>
        public async Task<IEnumerable<EventDto>> GetUserParticipatingEventsAsync(Guid userId)
        {
            IEnumerable<EventDto> userParticipatingEvents = await _userRepository
                .WithIncludes(u => u.Participations) 
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Participations) 
                .Select(p => new EventDto() 
                {
                    Id = p.Event.Id.ToString(),
                    Title = p.Event.Title,
                    Description = p.Event.Description,
                    StartDate = p.Event.StartTime,
                    EndDate = p.Event.EndTime,
                    CreatorId = p.Event.CreatorId.ToString(),
                    IsCancelled = p.Event.IsCancelled,
                    Participants = p.Event.Participants.Select(participant => new UserDto()
                    {
                        Id = participant.UserId.ToString(),
                        Email = participant.User.Email,
                        UserName = participant.User.UserName
                    }).ToList()
                })
                .ToListAsync();

            return userParticipatingEvents;
        }

        /// <summary>
        /// Check if a user has any active events.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Returns true if user has any active events or false if user hasn't</returns>
        public async Task<bool> CheckIfUserHasActiveEvents(Guid userId)
        {
            bool isUserHavingActiveEvents = await _eventRepository
                .ExistsByExpressionAsync(e => e.CreatorId == userId && e.IsCancelled == false);

            return isUserHavingActiveEvents;
        }

        /// <summary>
        /// Retrieves events created by the specified user with optional filtering.
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="filter">Optional filter criteria for the events</param>
        /// <returns>A collection of <see cref="EventDto"/> matching the specified criteria</returns>
        public async Task<IEnumerable<EventDto>> GetUserEventsAsync(Guid userId, EventFilterCriteriaDto? filter = null)
        {
            //IQueryable<Event> query = _eventRepository
            //    .WithIncludes(e => e.Participants, e => e.Participants.Select(p => p.User))
            //    .Where(e => e.CreatorId == userId);

            IQueryable<Event> query = _eventRepository
                .WithIncludes(u => u.Participants);

            query = query.Include(e => e.Participants)
                .ThenInclude(p => p.User)
                .Where(e => e.CreatorId == userId);

            if (filter != null)
            {
                if (filter.IsCancelled.HasValue)
                {
                    query = query.Where(e => e.IsCancelled == filter.IsCancelled.Value);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(e => e.StartTime >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(e => e.EndTime <= filter.EndDate.Value);
                }
            }

            IEnumerable<EventDto> events = await query
                .Select(e => new EventDto
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
                }).ToListAsync();

            return events;
        }

        /// <summary>
        /// Retrieves users based on specified filter criteria
        /// </summary>
        /// <param name="filter">Optional filter criteria for users</param>
        /// <returns>A collection of filtered users</returns>
        public async Task<IEnumerable<UserDtoExtended>> GetUsersAsync(UserFilterCriteriaDto? filter = null)
        {
            IQueryable<User> query = _userRepository
                .WithIncludes(
                    u => u.CreatedEvents,
                    u => u.Participations);

            query = query
                .Include(u => u.Participations)
                .ThenInclude(p => p.Event)
                .ThenInclude(e => e.Participants)
                .ThenInclude(ep => ep.User);

            if (filter != null)
            {
                // Apply username filter if provided
                if (!string.IsNullOrWhiteSpace(filter.Username))
                {
                    query = query.Where(u => u.UserName.Contains(filter.Username));
                }

                // Apply email filter if provided
                if (!string.IsNullOrWhiteSpace(filter.Email))
                {
                    query = query.Where(u => u.Email.Contains(filter.Email));
                }

                // Apply active events filter if provided
                if (filter.HasActiveEvents.HasValue)
                {
                    if (filter.HasActiveEvents.Value == true)
                    {
                        // User has active events (not cancelled)
                        query = query.Where(u => u.CreatedEvents.Any(e => !e.IsCancelled));
                    }
                    else
                    {
                        // User has no active events (all events are cancelled or no events)
                        query = query.Where(u => u.CreatedEvents.Any(e => e.IsCancelled));
                    }
                }
            }

            IEnumerable<UserDtoExtended> users = await query
                .Select(u => new UserDtoExtended()
                {
                    Id = u.Id.ToString(),
                    UserName = u.UserName,
                    Email = u.Email,
                    CreatedEvents = u.CreatedEvents.Select(e => new EventDto
                    {
                        Id = e.Id.ToString(),
                        Title = e.Title,
                        Description = e.Description,
                        StartDate = e.StartTime,
                        EndDate = e.EndTime,
                        CreatorId = e.CreatorId.ToString(),
                        IsCancelled = e.IsCancelled,
                        Participants = e.Participants.Select(p => new UserDto
                        {
                            Id = p.UserId.ToString(),
                            UserName = p.User.UserName,
                            Email = p.User.Email
                        }).ToList()
                    }).ToList(),
                    ParticipatingEvents = u.Participations.Select(p => new EventDto
                    {
                        Id = p.Event.Id.ToString(),
                        Title = p.Event.Title,
                        Description = p.Event.Description,
                        StartDate = p.Event.StartTime,
                        EndDate = p.Event.EndTime,
                        CreatorId = p.Event.CreatorId.ToString(),
                        IsCancelled = p.Event.IsCancelled,
                        Participants = p.Event.Participants.Select(participant => new UserDto
                        {
                            Id = participant.UserId.ToString(),
                            UserName = participant.User.UserName,
                            Email = participant.User.Email
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return users;
        }
    }
}
