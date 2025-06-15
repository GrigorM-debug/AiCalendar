using System.Runtime.CompilerServices;
using AiCalendar.WebApi.DTOs.Users;
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] LoginAndRegisterInputDto input)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
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
        public async Task<IActionResult> Logic([FromBody] LoginAndRegisterInputDto input)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
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
        /// <param name="id">The ID of the user to update (GUID format).</param>
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
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            if(User.Identity == null || !User.Identity.IsAuthenticated || User.FindFirst("id")?.Value != userId.ToString())
            {
                return Forbid();
            }

            //Check if user exists
            bool userExists = await _userService.UserExistsByIdAsync(userId);

            if (!userExists)
            {
                return NotFound();
            }

            try
            {
                UserDto updatedUser = await _userService.UpdateUserAsync(userId, updateUserDto);

                return Ok(updatedUser);
            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("password"))
                {
                    return BadRequest(new {error = ex.Message});
                }
                _logger.LogError(ex, "Error updating user with ID {UserId}.", userId);
                return StatusCode(500, "Internal server error while updating user.");
            }
        }
    }
}
