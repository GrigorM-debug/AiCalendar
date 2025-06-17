using System.Runtime.CompilerServices;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Extensions;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCalendar.WebApi.Controllers
{
    /// <summary>
    /// Controller for handling user registration, login, updating and deleting.
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenProvider _tokenProvider;

        public UserController(ILogger<UserController> logger, IUserService userService, IPasswordHasher passwordHasher, ITokenProvider tokenProvider)
        {
            _logger = logger;
            _userService = userService;
            _passwordHasher = passwordHasher;
            _tokenProvider = tokenProvider;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="input">The user registration information.</param>
        /// <returns>Returns a 201 Created response with user info or conflict if username already exists.</returns>
        /// <remarks>
        /// Sample request:
        ///     Post /api/v1/user/register
        ///     {
        ///         "UserName": "newuser",
        ///         "Email": "newuser@example.come",
        ///         "Password": "SecurePassword123"
        ///     }
        /// </remarks>
        /// <response code="201">User created successfully.</response>
        /// <response code="403">User is already authenticated.</response>
        /// <response code="409">User with the same username already exists.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] LoginAndRegisterInputDto input)
        {
            if (User.Identity != null || User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User is already authenticated. Cannot register a new account.");
                return Forbid("User is already authenticated. Please log out before registering a new account.");
            }

            bool userExists = await _userService.UserExistsByUsernameAsync(input.UserName);

            if (userExists)
            {
                _logger.LogWarning("User with username {UserName} already exists.", input.UserName);
                return Conflict($"User with this username '{input.UserName}' already exists.");
            }

            UserDto user = await _userService.RegisterAsync(input);

            _logger.LogInformation("User {UserName} registered successfully.", user.UserName);

            return StatusCode(201, user);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="input">The user login credentials.</param>
        /// <returns>Returns a JWT token and user details if credentials are valid.</returns>
        /// <remarks>
        /// Sample request:
        ///     POST /api/v1/user/login
        ///     {
        ///         "UserName": "Heisenberg",
        ///         "Email": "heisenberg@example.com",
        ///         "Password": "hashedpassword456"
        ///     }
        /// </remarks>
        /// <response code="200">Login successful.</response>
        /// <response code="403">User is already authenticated.</response>
        /// <response code="404">User not found with provided username and email.</response>
        /// <response code="401">Invalid password.</response>
        /// 
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginAndRegisterInputDto input)
        {
            if (User.Identity != null || User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User is already authenticated. Cannot log in again.");
                return Forbid();
            }

            User? user = await _userService.GetUserByUserNameAndEmail(input.UserName, input.Email);

            if(user == null)
            {
                _logger.LogWarning("User with username {UserName} and email {Email} not found.", input.UserName, input.Email);
                return NotFound("User not found.");
            }

            //Validate password
            bool isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHashed, input.Password);

            if (!isPasswordValid)
            {
                _logger.LogWarning("Invalid password for user {UserName}.", user.UserName);
                return Unauthorized("Invalid password.");
            }
            
            string token = _tokenProvider.GenerateToken(user);

            _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);

            LoginResponseDto response = new LoginResponseDto
            {
                UserId = user.Id.ToString(),
                Username = user.UserName,
                Email = user.Email,
                Token = token
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates the details of an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update</param>
        /// <param name="updateUserDto">The updated user information.</param>
        /// <returns>Returns the updated user object if successful.</returns>
        /// <remarks>
        /// Sample request:
        ///     Put /api/v1/user/A1B2C3D4-E5F6-7890-1234-567890ABCDEF
        ///     {
        ///         UserName: "adminEdited",
        ///         Email: "admin@example.com",
        ///         OldPassword: "hashedpassword123",
        ///         NewPassword: "NewSecurePassword456"
        ///     }
        /// </remarks>
        /// <response code="200">User updated successfully.</response>
        /// <response code="400">Invalid user ID format or bad input (e.g., invalid password).</response>
        /// <response code="403">User is not authorized to update this account.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Unexpected server error during update.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            string? currentUserIdString = User.GetUserId();

            if (User.Identity == null || !User.Identity.IsAuthenticated || currentUserIdString == null)
            {
                _logger.LogWarning("Unauthorized access attempt to update user with ID {UserId}.", id);
                return Forbid("You are not authorized to delete this account.");
            }

            if (!Guid.TryParse(id, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}.", id);
                return BadRequest("Invalid user ID format.");
            }

            if (!Guid.TryParse(currentUserIdString, out Guid currentUserId))
            {
                _logger.LogWarning("Invalid current user ID format: {CurrentUserId}.", currentUserIdString);
                return BadRequest("Invalid user ID format.");
            }

            if (currentUserId != userId)
            {
                _logger.LogWarning("User with ID {CurrentUserId} attempted to update user with ID {UserId} without authorization.", currentUserId, userId);
                return Forbid("You are not authorized to delete this account.");
            }

            //Check if user exists
            bool userExists = await _userService.UserExistsByIdAsync(userId);

            if (!userExists)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return NotFound("User no found.");
            }

            try
            {
                UserDto updatedUser = await _userService.UpdateUserAsync(userId, updateUserDto);

                _logger.LogInformation("User with ID {UserId} updated successfully.", userId);

                return Ok(updatedUser);
            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("password"))
                {
                    _logger.LogWarning(ex, "Error updating user with ID {UserId}: {Message}", userId, ex.Message);
                    return BadRequest(new {error = ex.Message});
                }
                _logger.LogError(ex, "Error updating user with ID {UserId}.", userId);
                return StatusCode(500, "Internal server error while updating user.");
            }
        }

        /// <summary>
        /// Retrieves all events where the authenticated user is a participant
        /// </summary>
        /// <response code="200">Returns the list of events where the user is a participant</response>
        /// <response code="401">User is not authenticated</response>
        /// <returns>A collection of events where the user is a participant</returns>
        [HttpGet("participating-events")]
        [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserParticipatingEvents()
        {
            string? id = User.GetUserId();

            if (User.Identity == null || !User.Identity.IsAuthenticated || id == null)
            {
                _logger.LogWarning("Unauthorized access attempt to get user participating events.");
                return Forbid("You are not authorized to access this resource.");
            }

            if (!Guid.TryParse(id, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}.", id);
                return BadRequest("Invalid user ID format.");
            }

            bool userExists = await _userService.UserExistsByIdAsync(userId);

            if (!userExists)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return NotFound("User not found.");
            }

            IEnumerable<EventDto> userParticipatingEvents = await _userService.GetUserParticipatingEventsAsync(userId);

            return Ok(userParticipatingEvents);
        }

        /// <summary>
        /// Deletes the authenticated user's account
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        /// <response code="204">User was successfully deleted</response>
        /// <response code="400">User has active events that need to be cancelled first</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">User is not found</response>
        /// <response code="403">User is not authorized</response>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            string? currentUserIdString = User.GetUserId();

            if (User.Identity == null || !User.Identity.IsAuthenticated || currentUserIdString == null)
            {
                _logger.LogWarning("Unauthorized access attempt to delete user with ID {UserId}.", id);
                return Forbid("You are not authorized to access this resource.");
            }

            if (!Guid.TryParse(currentUserIdString, out Guid currentUserId))
            {
                _logger.LogWarning("Invalid current user ID format: {CurrentUserId}.", currentUserIdString);
                return BadRequest("Invalid user ID format.");
            }

            if (!Guid.TryParse(id, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}.", id);
                return BadRequest("Invalid user ID format.");
            }

            if (currentUserId != userId)
            {
                _logger.LogWarning("User with ID {CurrentUserId} attempted to delete user with ID {UserId} without authorization.", currentUserId, userId);
                return Forbid("You are not authorized to delete this account.");
            }

            bool userExists = await _userService.UserExistsByIdAsync(userId);
            if (!userExists)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return NotFound("User not found.");
            }

            //Check if user has active events
            bool isUserHasActiveEvents = await _userService.CheckIfUserHasActiveEvents(userId);

            if (isUserHasActiveEvents)
            {
                _logger.LogWarning("User with ID {UserId} has active events and cannot be deleted.", userId);
                return BadRequest("User has active events. Please cancel them before deleting your account.");
            }

            await _userService.DeleteUserAsync(userId);

            _logger.LogInformation("User with ID {UserId} deleted successfully.", userId);

            return NoContent();
        }

        /// <summary>
        /// Gets user events with optional filtering
        /// </summary>
        /// <param name="filter">Optional filter criteria for the events</param>
        /// <returns>Returns collection of filtered user events</returns>
        /// <response code="200">Returns the filtered list of events</response>
        /// <response code="400">Invalid user ID format</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">User is not authorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("events")]
        [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserEvents([FromBody] EventFilterCriteriaDto? filter)
        {
            string? id = User.GetUserId();

            if (User.Identity == null || !User.Identity.IsAuthenticated || id == null)
            {
                _logger.LogWarning("Unauthorized access attempt to get user events.");
                return Forbid("You are not authorized to access this resource.");
            }

            if (!Guid.TryParse(id, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}.", id);
                return BadRequest("Invalid user ID format.");
            }

            bool userExists = await _userService.UserExistsByIdAsync(userId);
            if (!userExists)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return NotFound("User not found.");
            }

            IEnumerable<EventDto> events = await _userService.GetUserEventsAsync(userId, filter);

            return Ok(events);
        }

        /// <summary>
        /// Gets users with optional filtering
        /// </summary>
        /// <param name="filter">Optional filter criteria for users</param>
        /// <returns>Returns collection of filtered users</returns>
        /// <response code="200">Returns the filtered list of users</response>
        [AllowAnonymous]
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<UserDtoExtended>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromBody] UserFilterCriteriaDto? filter)
        {
            IEnumerable<UserDtoExtended> users = await _userService.GetUsersAsync(filter);

            return Ok(users);
        }
    }
}
