using AiCalendar.WebApi.Controllers;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

        [Test]
        public async Task GetUserParticipatingEvents_ShouldReturnUnAuthorized_WhenUsersIsUnAuthorized()
        {
            _userController.ControllerContext = new ControllerContext();
            _userController.ControllerContext.HttpContext = new DefaultHttpContext();
            _userController.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            var result = await _userController.GetUserParticipatingEvents();
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You are not authorized to access this resource."));
        }

        [Test]
        public async Task GetUserParticipatingEvents_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "someemail@example.com"),
                new Claim(ClaimTypes.Name, "someusername")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.GetUserParticipatingEvents();
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task GetUserParticipatingEvents_ShouldReturnBadRequest_WhenUserIdInInvalid()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid guid"),
                new Claim(ClaimTypes.Email, "someemail@example.com"),
                new Claim(ClaimTypes.Name, "someusername")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.GetUserParticipatingEvents();
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid user ID format."));
        }

        [Test]
        public async Task GetUserParticipatingEvents_ShouldReturnEmptyCollection_WhenUserHasNoParticipatingEvents()
        {
            var newUser = new LoginAndRegisterInputDto()
            {
                Email = "newuser@example.com",
                UserName = "new user",
                Password = "password123"
            };

            var newUserData = await _userService.RegisterAsync(newUser);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, newUserData.Id),
                new Claim(ClaimTypes.Email, newUserData.Email),
                new Claim(ClaimTypes.Name, newUserData.UserName)
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.GetUserParticipatingEvents();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var badRequestResult = result as OkObjectResult;
            Assert.That(badRequestResult.Value, Is.Empty);
        }

        [Test]
        public async Task GetUserParticipatingEvents_ShouldReturnUserParticipatingEvents()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.GetUserParticipatingEvents();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var participatingEvents = okResult.Value as List<EventDto>;
            Assert.That(participatingEvents, Is.Not.Empty);
            Assert.That(participatingEvents.Count, Is.EqualTo(3)); // Assuming the user has 2 participating events
            Assert.That(
                participatingEvents.All(e =>
                    e.Participants.Any(p => p.Id == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower())),
                Is.True); // All events should have the user as a participant
        }
        #endregion

        #region DeleteUser

        [Test]
        public async Task DeleteUser_ShouldReturnUnAuthorized_WhenUserIsUnAuthorized()
        {
            _userController.ControllerContext = new ControllerContext();
            _userController.ControllerContext.HttpContext = new DefaultHttpContext();
            _userController.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            var result = await _userController.DeleteUser("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower());
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You are not authorized to access this resource."));
        }

        [Test]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "nonexisting@example.com"),
                new Claim(ClaimTypes.Name, "Non existing")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };
            var result = await _userController.DeleteUser(userId);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task DeleteUser_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid guid"),
                new Claim(ClaimTypes.Email, "invalid@example.com"),
                new Claim(ClaimTypes.Name, "Invalid User")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.DeleteUser("invalid guid");
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid user ID format."));
        }

        [Test]
        public async Task DeleteUser_ShouldReturnForbidden_WhenUserTriesToDeleteOtherUserAccount()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.DeleteUser("F0E9D8C7-B6A5-4321-FEDC-BA9876543210".ToLower());
            Assert.That(result, Is.InstanceOf<ForbidResult>());
            var forbidResult = result as ForbidResult;
            Assert.That(forbidResult, Is.Not.Null);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnBadRequest_WhenUserHasActiveEvents()
        {
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user1Id.ToString()),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.DeleteUser(user1Id.ToString().ToLower());
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value,
                Is.EqualTo("User has active events. Please cancel them before deleting your account."));
        }

        [Test]
        public async Task DeleteUser_ShouldDeleteUserSuccessfully_WhenUserHasNoActiveEvents()
        {
            var newUser = new LoginAndRegisterInputDto()
            {
                Email = "newuser@example.com",
                UserName = "New User",
                Password = "password123"
            };

            var newUserData = await _userService.RegisterAsync(newUser);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, newUserData.Id),
                new Claim(ClaimTypes.Email, newUserData.Email),
                new Claim(ClaimTypes.Name, newUserData.UserName)
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.DeleteUser(newUserData.Id.ToLower());
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
        #endregion

        #region GetUserEvents

        [Test]
        public async Task GetUserEvents_ShouldReturnUnAuthorized_WhenUserIsUnAuthorized()
        {
            _userController.ControllerContext = new ControllerContext();
            _userController.ControllerContext.HttpContext = new DefaultHttpContext();
            _userController.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            var result = await _userController.GetUserEvents(null);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You are not authorized to access this resource."));
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "ivan@example.com"),
                new Claim(ClaimTypes.Name, "ivan76")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.GetUserEvents(null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnEmptyCollection_WhenUserHasNoEvents()
        {
            var newUser = new LoginAndRegisterInputDto()
            {
                UserName = "Ivan7630",
                Email = "ivan@example.com",
                Password = "newpassword456"
            };
            var newUserData = await _userService.RegisterAsync(newUser);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, newUserData.Id),
                new Claim(ClaimTypes.Email, newUserData.Email),
                new Claim(ClaimTypes.Name, newUserData.UserName)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.GetUserEvents(null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Empty);
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnUserEvents()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var result = await _userController.GetUserEvents(null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Not.Empty);
            Assert.That(userEvents.Count(), Is.EqualTo(2));
            Assert.That(userEvents.All(e => e.CreatorId == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()),
                Is.True); // All events should be created by the user
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnFilteredEvents_WhenStartDateFilterIsApplied()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)
            };

            var result = await _userController.GetUserEvents(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Not.Empty);
            Assert.That(userEvents.Count(), Is.EqualTo(2));
            Assert.That(userEvents.First().StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));

            Assert.That(userEvents.All(e => e.CreatorId == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()),
                Is.True); // All events should be created by the user
            Assert.That(userEvents.All(e => e.StartDate >= new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnEmptyCollection_WhenThereAreNoEventsMatchingTheStartDateFilter()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2030, 6, 16, 9, 0, 0, DateTimeKind.Utc)
            };

            var result = await _userController.GetUserEvents(filter);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Empty);
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnAllEvents_MatchingTheEndDateFilter()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                EndDate = new DateTime(2025, 6, 16, 10, 0, 0, DateTimeKind.Utc)
            };

            var result = await _userController.GetUserEvents(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Not.Empty);
            Assert.That(userEvents.Count(), Is.EqualTo(1));
            Assert.That(userEvents.All(e => e.CreatorId == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()),
                Is.True); // All events should be created by the user
            Assert.That(userEvents.All(e => e.EndDate <= new DateTime(2025, 6, 16, 10, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnEmptyCollection_WhenThereAreNoEventsMatchingTheEndDateFilter()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                EndDate = new DateTime(2018, 6, 16, 10, 0, 0, DateTimeKind.Utc)
            };

            var result = await _userController.GetUserEvents(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Empty);
            Assert.That(userEvents.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnFilteredEvents_WhenStartDateAndEndDateFilterAreApplied()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 6, 16, 10, 0, 0, DateTimeKind.Utc)
            };

            var result = await _userController.GetUserEvents(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Not.Empty);
            Assert.That(userEvents.Count(), Is.EqualTo(1)); // Only one event matches the filter
            Assert.That(userEvents.All(e => e.CreatorId == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()),
                Is.True); // All events should be created by the user
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnEmptyCollection_WhenThereAreNoEventsMatchingTheStartAndEndDateFilter()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2030, 6, 16, 9, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2030, 6, 16, 10, 0, 0, DateTimeKind.Utc)
            };

            var result = await _userController.GetUserEvents(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Empty);
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnAllEvents_MatchingTheIsCancelFilter()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                IsCancelled = true
            };

            await _eventService.CancelEventAsync(Guid.Parse("E1000000-0000-0000-0000-000000000001"),
                Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"));

            var result = await _userController.GetUserEvents(filter);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Not.Empty);
            Assert.That(userEvents.Count(), Is.EqualTo(1)); // Assuming the user has 1 cancelled event
            Assert.That(userEvents.All(e => e.IsCancelled), Is.True); // All events should be cancelled
        }

        [Test]
        public async Task GetUserEvents_ShouldReturnEmptyCollection_WhenThereAreNoEventsMatchingTheIsCancelFilter()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            var filter = new EventFilterCriteriaDto()
            {
                IsCancelled = true
            };

            var result = await _userController.GetUserEvents(filter);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var userEvents = okResult.Value as IEnumerable<EventDto>;
            Assert.That(userEvents, Is.Empty);
        }
        #endregion

        #region UpdateUser

        [Test]
        public async Task UpdateUser_ShouldReturnUnAuthorized_WhenUserIsUnAuthorized()
        {
            _userController.ControllerContext = new ControllerContext();
            _userController.ControllerContext.HttpContext = new DefaultHttpContext();
            _userController.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updatedUser = new UpdateUserDto()
            {
                Email = "maikati@example.com",
                UserName = "maikati",
                OldPassword = "hashedpassword123",
                NewPassword = "newhashedpassword123"
            };

            var result = await _userController.UpdateUser(userId, updatedUser);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You are not authorized to delete this account."));
        }

        [Test]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = Guid.NewGuid().ToString();
            var updatedUser = new UpdateUserDto()
            {
                Email = "adminEdit@example.com",
            };

            var result = await _userController.UpdateUser(userId, updatedUser);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User no found."));
        }

        [Test]
        public async Task UpdateUser_ShouldReturnNotFound_WhenTheCurrentUserDoesNotExists()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "example@example.com"),
                new Claim(ClaimTypes.Name, "example")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";
            var updatedUser = new UpdateUserDto()
            {
                Email = "newemail@example.com"
            };

            var result = await _userController.UpdateUser(userId, updatedUser);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("Current user not found."));
        }

        [Test]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "invalid id";
            var updatedUser = new UpdateUserDto()
            {
                Email = "newemail@example.com"
            };

            var result = await _userController.UpdateUser(userId, updatedUser);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid user ID format."));
        }

        [Test]
        public async Task UpdateUser_ShouldReturnForbid_WhenTheCurrentUserAndUserGettingUpdatedAreDifferent()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "F0E9D8C7-B6A5-4321-FEDC-BA9876543210";
            var updatedUser = new UpdateUserDto()
            {
                Email = "newEmail@example.com"
            };

            var result = await _userController.UpdateUser(userId, updatedUser);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task UpdateUser_ShouldUpdateUserSuccessfully_WhenTheAllDataIsProvided()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
                Email = "admin_update@example.com",
                UserName = "admin_updated",
                NewPassword = "hashedpassword1234",
                OldPassword = "hashedpassword123"
            };

            var result = await _userController.UpdateUser(userId, updateUser);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var updatedUser = result as OkObjectResult;

            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.Value, Is.InstanceOf<UserDto>());
            var userDto = updatedUser.Value as UserDto;
            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(userId.ToLower()));
            Assert.That(userDto.Email, Is.EqualTo(updateUser.Email));
            Assert.That(userDto.UserName, Is.EqualTo(updateUser.UserName));
        }

        [Test]
        public async Task UpdateUser_ShouldUpdateOnlyTheUsername_WhenUsernameOnlyIsProvided()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
                UserName = "admin_updated",
            };

            var result = await _userController.UpdateUser(userId, updateUser);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var updatedUser = result as OkObjectResult;
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.Value, Is.InstanceOf<UserDto>());
            var userDto = updatedUser.Value as UserDto;
            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(userId.ToLower()));
            Assert.That(userDto.UserName, Is.EqualTo(updateUser.UserName));
            Assert.That(userDto.Email, Is.EqualTo("admin@example.com"));
        }

        [Test]
        public async Task UpdateUser_ShouldUpdateOnlyTheEmail_WhenEmailOnlyIsProvided()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
                Email = "admin_updated@example.com"
            };

            var result = await _userController.UpdateUser(userId, updateUser);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var updatedUser = result as OkObjectResult;
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.Value, Is.InstanceOf<UserDto>());
            var userDto = updatedUser.Value as UserDto;
            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(userId.ToLower()));
            Assert.That(userDto.Email, Is.EqualTo(updateUser.Email));
            Assert.That(userDto.UserName, Is.EqualTo("admin"));
        }

        [Test]
        public async Task UpdateUser_ShouldUpdateOnlyThePassword_WhenPasswordIsProvided()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
                OldPassword = "hashedpassword123",
                NewPassword = "newhashedpassword123"
            };

            var result = await _userController.UpdateUser(userId, updateUser);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var updatedUser = result as OkObjectResult;
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.Value, Is.InstanceOf<UserDto>());
            var userDto = updatedUser.Value as UserDto;
            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(userId.ToLower()));
            Assert.That(userDto.Email, Is.EqualTo("admin@example.com"));
            Assert.That(userDto.UserName, Is.EqualTo("admin"));

            // Verify that the password was updated
            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));

            Assert.That(user, Is.Not.Null);

            bool isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHashed, updateUser.NewPassword);

            Assert.That(isPasswordValid, Is.True);
        }

        [Test]
        public async Task UpdateUser_ShouldUpdateUsernameAndPassword_WhenTheyAreProvided()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
               UserName = "new_username",
               OldPassword = "hashedpassword123",
               NewPassword = "newhashedpassword123"
            };

            var result = await _userController.UpdateUser(userId, updateUser);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var updatedUser = result as OkObjectResult;
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.Value, Is.InstanceOf<UserDto>());
            var userDto = updatedUser.Value as UserDto;
            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(userId.ToLower()));
            Assert.That(userDto.Email, Is.EqualTo("admin@example.com"));
            Assert.That(userDto.UserName, Is.EqualTo(userDto.UserName));

            // Verify that the password was updated
            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));

            Assert.That(user, Is.Not.Null);

            bool isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHashed, updateUser.NewPassword);

            Assert.That(isPasswordValid, Is.True);
        }

        [Test]
        public async Task UpdateUser_ShouldUpdateEmailAndPassword_WhenTheyAreProvided()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
                Email = "newEmail@example.com",
                OldPassword = "hashedpassword123",
                NewPassword = "newhashedpassword123"
            };

            var result = await _userController.UpdateUser(userId, updateUser);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var updatedUser = result as OkObjectResult;
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.Value, Is.InstanceOf<UserDto>());
            var userDto = updatedUser.Value as UserDto;
            Assert.That(userDto, Is.Not.Null);
            Assert.That(userDto.Id, Is.EqualTo(userId.ToLower()));
            Assert.That(userDto.Email, Is.EqualTo(updateUser.Email));
            Assert.That(userDto.UserName, Is.EqualTo("admin"));

            // Verify that the password was updated
            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));

            Assert.That(user, Is.Not.Null);

            bool isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHashed, updateUser.NewPassword);

            Assert.That(isPasswordValid, Is.True);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenOldPasswordIsIncorrect()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
                OldPassword = "hashedpassword123dwdwdwdwd",
                NewPassword = "newhashedpassword123"
            };

            var result = await _userController.UpdateUser(userId, updateUser);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value.GetType().GetProperty("error").GetValue(badRequestResult.Value), Is.EqualTo("Old password is incorrect."));
        }

        [Test]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenOldPasswordAndNewPasswordAreTheSame()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin")
            };

            var identity = new ClaimsIdentity(claims);

            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = principal
                }
            };

            string userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF";

            var updateUser = new UpdateUserDto()
            {
                OldPassword = "hashedpassword123",
                NewPassword = "hashedpassword123"
            };

            var result = await _userController.UpdateUser(userId, updateUser);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value.GetType().GetProperty("error").GetValue(badRequestResult.Value), Is.EqualTo("New password cannot be the same as the old password."));
        }

        #endregion
    }
}
