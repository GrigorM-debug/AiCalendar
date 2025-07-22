using AiCalendar.WebApi.Controllers;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace AiCalendar.Tests
{
    public class UserControllerTests : InMemoryDbTestBase
    {
        private ILogger<UserController> _logger;
        private IUserService _userService;
        private ITokenProvider _tokenProvider;
        private UserController _userController;
        private IRepository<Event> _eventRepository;
        private IRepository<User> _userRepository;
        private IRepository<Participant> _participantRepository;
        private IEventService _eventService;
        private IConfiguration _configuration;

        [SetUp]
        public async Task Setup()
        {
            await Init();
            _eventRepository = new Repository<Event>(_context);
            _userRepository = new Repository<User>(_context);
            _participantRepository = new Repository<Participant>(_context);
            _logger = new LoggerFactory().CreateLogger<UserController>();
            _userService = new UserService(_userRepository, _passwordHasher, _eventRepository, _participantRepository);
            _configuration = new ConfigurationManager();
            _tokenProvider = new TokenProvider(_configuration);
            _userController = new UserController(_logger, _userService, _passwordHasher, _tokenProvider);
            _eventService = new EventService(_eventRepository);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Dispose();
        }

        #region Register

        [Test]
        public async Task Register_ShouldRegisterNewUserSuccessfully()
        {
            var newUser = new LoginAndRegisterInputDto()
            {
                Email = "mohamed@example.com",
                UserName = "Mohamed",
                Password = "password123"
            };

            var result = await _userController.Register(newUser);

            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;

            // 2. Check the StatusCode
            Assert.That(objectResult.StatusCode, Is.EqualTo(201)); // Or (int)System.Net.HttpStatusCode.Created

            // 3. Check the value returned
            Assert.That(objectResult.Value, Is.InstanceOf<UserDto>());
            var returnedUser = objectResult.Value as UserDto;

            Assert.That(returnedUser.UserName, Is.EqualTo(newUser.UserName));
            Assert.That(returnedUser.Email, Is.EqualTo(newUser.Email));

        }

        [Test]
        public async Task Register_ShouldReturnForbidden_IfUserIsAuthenticated()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@example.com")
            };

            var identity = new ClaimsIdentity(claims);

            var claimPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = claimPrincipal
                }
            };

            var newUser = new LoginAndRegisterInputDto()
            {
                UserName = "Mohamed",
                Email = "mohamed@example.com",
                Password = "password123"
            };

            var result = await _userController.Register(newUser);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        [TestCase("admin")]
        [TestCase("Heisenberg")]
        [TestCase("JessiePinkman")]
        public async Task Register_ShouldReturnConflict_IfUserWithUsernameAlreadyExists(string username)
        {
            var newUser = new LoginAndRegisterInputDto()
            {
                UserName = username,
                Email = "admin@example.com",
                Password = "password123"
            };

            var result = await _userController.Register(newUser);

            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
            var conflictResult = result as ConflictObjectResult;
            Assert.That(conflictResult.StatusCode, Is.EqualTo(409)); // Or (int)System.Net.HttpStatusCode.Conflict
            Assert.That(conflictResult.Value, Is.EqualTo($"User with this username '{username}' already exists."));
        }
        #endregion

        #region Login
        [Test]
        public async Task Login_ShouldReturnForbidden_WhenUserIsAlreadyAuthenticated()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@example.com")
            };

            var identity = new ClaimsIdentity(claims);

            var claimPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = claimPrincipal
                }
            };

            var user = new LoginAndRegisterInputDto()
            {
                UserName = "admin",
                Email = "admin@example.com",
                Password = "hashedpassword123"
            };

            var result = await _userController.Login(user);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        [TestCase("NonExisting", "nonexisting@example.com")]
        [TestCase("NonExisting1", "nonexisting1@example.com")]
        [TestCase("NonExisting2", "nonexisting2@example.com")]
        public async Task Login_ShouldReturnNotFound_WhenUserDoesNotExists(string username, string email)
        {
            var user = new LoginAndRegisterInputDto()
            {
                Email = email,
                UserName = username,
                Password = "password123"
            };

            var result = await _userController.Login(user);
            
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var resultObj = result as NotFoundObjectResult;
            Assert.That(resultObj.Value, Is.EqualTo("User not found."));
        }

        [Test]
        [TestCase("wrongpassword123")]
        [TestCase("wrongpassword1234")]
        [TestCase("wrongpassword12345")]
        public async Task Login_ShouldReturnUnAuthorized_IfPasswordIsWrong(string password)
        {
            var user = new LoginAndRegisterInputDto()
            {
                UserName = "admin",
                Email = "admin@example.com",
                Password = password
            };

            var result = await _userController.Login(user);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var resultObj = result as UnauthorizedObjectResult;
            Assert.That(resultObj.Value, Is.EqualTo("Invalid password."));
        }

        [Test]
        [TestCase("admin", "admin@example.com", "hashedpassword123")]
        [TestCase("Heisenberg", "heisenberg@example.com", "hashedpassword456")]
        [TestCase("JessiePinkman", "jessie@example.com", "hashedpassword789")]
        public async Task Login_ShouldLoginUserSuccessfully(string username, string email, string password)
        {
            var user = new LoginAndRegisterInputDto()
            {
                Email = email,
                Password = password,
                UserName = username
            };

            var result = await _userController.Login(user);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var resultObj = result as OkObjectResult;
            Assert.That(resultObj.Value, Is.InstanceOf<LoginResponseDto>());
            var userData = resultObj.Value as LoginResponseDto;

            Assert.That(userData.Email, Is.EqualTo(user.Email));
            Assert.That(userData.Username, Is.EqualTo(user.UserName));
        }
        #endregion

        #region GetUsers
        [Test]
        public async Task GetUsers_ShouldReturnAllUsers()
        {
            var result = await _userController.GetUsers(null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(3)); // We seeded 3 users
        }

        [Test]
        [TestCase("admin")]
        [TestCase("Heisenberg")]
        [TestCase("JessiePinkman")]
        public async Task GetUsers_ShouldReturnFilteredUsers_WhenUsernameFilterIsProvided(string username)
        {
            var filter = new UserFilterCriteriaDto()
            {
                Username = username
            };

            var result = await _userController.GetUsers(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(1)); // Only one user matches the search term
            Assert.That(users[0].UserName, Is.EqualTo(username));
        }

        [Test]
        [TestCase("nonexistinguser")]
        [TestCase("anothernonexistinguser")]
        public async Task GetUsers_ShouldReturnEmptyList_WhenNoUsersMatchFilter(string username)
        {
            var filter = new UserFilterCriteriaDto()
            {
                Username = username
            };
            var result = await _userController.GetUsers(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(0)); // No users match the search term
        }

        [Test]
        [TestCase("admin@example.com")]
        [TestCase("heisenberg@example.com")]
        [TestCase("jessie@example.com")]
        public async Task GetUsers_ShouldReturnFilteredUsers_WhenEmailFilterIsApplied(string email)
        {
            var filter = new UserFilterCriteriaDto()
            {
                Email = email
            };

            var result = await _userController.GetUsers(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(1));
            Assert.That(users[0].Email, Is.EqualTo(email));
        }

        [Test]
        [TestCase("nonexisting@example.com")]
        [TestCase("nonexisting2@example.com")]
        [TestCase("nonexisting3@example.com")]
        public async Task GetUsers_ShouldReturnEmptyCollection_WhenNoUsersMatchEmailFilter(string email)
        {
            var filter = new UserFilterCriteriaDto()
            {
                Email = email
            };

            var result = await _userController.GetUsers(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(0));
        }

        [Test]
        [TestCase("admin", "admin@example.com")]
        [TestCase("Heisenberg", "heisenberg@example.com")]
        [TestCase("JessiePinkman", "jessie@example.com")]
        public async Task GetUsers_ShouldReturnUsersMatchingTheFilter_WhenAllFilterAreApplied(string username,
            string email)
        {
            var filter = new UserFilterCriteriaDto()
            {
                Email = email,
                Username = username,
                HasActiveEvents = true
            };

            var result = await _userController.GetUsers(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(1));
            Assert.That(users[0].UserName, Is.EqualTo(username));
            Assert.That(users[0].Email, Is.EqualTo(email));
            Assert.That(users[0].CreatedEvents
                .All(e => e.IsCancelled == false)); // Assuming all seeded users have active events
        }

        [Test]
        [TestCase("nonexistinguser", "nonexisting@example.com")]
        [TestCase("anothernonexistinguser", "anothernonexistinguser@example.com")]
        [TestCase("yetanothernonexistinguser", "yetanothernonexistinguser@example.com")]
        public async Task GetUsers_ShouldReturnEmptyList_WhenNoUsersMatchAllFilters(string username, string email)
        {
            var filter = new UserFilterCriteriaDto()
            {
                Email = email,
                Username = username,
                HasActiveEvents = true
            };
            var result = await _userController.GetUsers(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUsers_ShouldReturnUsersWithCancelledEvents_WhenTheFilterIsApplied()
        {
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            Guid event1Id = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            await _eventService.CancelEventAsync(event1Id, user1Id);

            var filter = new UserFilterCriteriaDto()
            {
                HasActiveEvents = false
            };

            var result = await _userController.GetUsers(filter);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var objectResult = result as OkObjectResult;
            Assert.That(objectResult.Value, Is.InstanceOf<List<UserDtoExtended>>());
            var users = objectResult.Value as List<UserDtoExtended>;
            Assert.That(users.Count, Is.EqualTo(1)); // Only one user has a cancelled event
            Assert.That(users[0].CreatedEvents.Count, Is.EqualTo(2)); // The user has one cancelled event
            Assert.That(users[0].CreatedEvents.Any(e => e.IsCancelled)); // The event is cancelled
            Assert.That(users[0].CreatedEvents.First().Id, Is.EqualTo(event1Id.ToString()));
        }
        #endregion

        #region GetUserParticipatingEvents



        #endregion
    }
}
