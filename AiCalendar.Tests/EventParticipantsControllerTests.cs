using AiCalendar.WebApi.Controllers;
using AiCalendar.WebApi.Services.EventParticipants.Interfaces;
using AiCalendar.WebApi.Services.Events.Interfaces;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.EventParticipants;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCalendar.Tests
{
    public class EventParticipantsControllerTests : InMemoryDbTestBase
    {
        private ILogger<EventParticipantsController> _logger;
        private IEventService _eventService;
        private IEventParticipantsService _eventParticipantsService;
        private IUserService _userService;
        private IRepository<Event> _eventRepository;
        private IPasswordHasher _passwordHasher;
        private IRepository<Participant> _participantRepository;
        private IRepository<User> _userRepository;
        private EventParticipantsController _controller;

        [SetUp]
        public async Task Setup()
        {
            await Init();
            _userRepository = new Repository<User>(_context);
            _participantRepository = new Repository<Participant>(_context);
            _passwordHasher = new PasswordHasher();
            _logger = new Logger<EventParticipantsController>(new LoggerFactory());
            _eventRepository = new Repository<Event>(_context);
            _eventService = new EventService(_eventRepository);
            _eventParticipantsService = new EventParticipantsService(_participantRepository);
            _userService = new UserService(_userRepository, _passwordHasher, _eventRepository, _participantRepository);

            _controller = new EventParticipantsController(
                _logger,
                _eventService,
                _eventParticipantsService,
                _userService
            );

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@example.com")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");

            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [TearDown]
        public async Task TearDown()
        {
            await Dispose();
        }

        #region GetEventParticipantsAsync
        [Test]
        public async Task GetEventParticipantsAsync_ShouldReturnOkResult_WhenAuthenticated()
        {
            string eventId = "E1000000-0000-0000-0000-000000000001".ToLower();
            var result = await _controller.GetEventParticipantsAsync(eventId);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult?.Value, Is.InstanceOf<IEnumerable<UserDto>>());
            var participants = okResult?.Value as IEnumerable<UserDto>;
            Assert.That(participants, Is.Not.Empty);

            Assert.That(participants.Count(), Is.EqualTo(3));

            Assert.That(participants.Any(p => p.Id == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(participants.Any(p => p.Id == "F0E9D8C7-B6A5-4321-FEDC-BA9876543210".ToLower()));
            Assert.That(participants.Any(p => p.Id == "11223344-5566-7788-99AA-BBCCDDEEFF00".ToLower()));

            Assert.That(participants.Any(p => p.UserName == "admin"));
            Assert.That(participants.Any(p => p.UserName == "Heisenberg"));
            Assert.That(participants.Any(p => p.UserName == "JessiePinkman"));

            Assert.That(participants.Any(p => p.Email == "admin@example.com"));
            Assert.That(participants.Any(p => p.Email == "heisenberg@example.com"));
            Assert.That(participants.Any(p => p.Email == "jessie@example.com"));
        }

        [Test]
        public async Task GetEventParticipantsAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
        {
            string eventId = "00000000-0000-0000-0000-000000000000"; // Non-existent event ID
            var result = await _controller.GetEventParticipantsAsync(eventId);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetEventParticipantsAsync_ShouldReturnBadRequest_WhenEventIdIsInvalid()
        {
            string eventId = "invalid-id"; // Invalid event ID
            var result = await _controller.GetEventParticipantsAsync(eventId);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("Invalid eventId format."));
        }

        [Test]
        public async Task GetEventParticipant_ShouldReturnForbidden_WhenUserIsNotEventParticipantOrEventCreator()
        {
            var newUser = new LoginAndRegisterInputDto()
            {
                Email = "newuser@wxample.com",
                UserName = "newuser",
                Password = "newpassword123"
            };

            var newUserData = await _userService.RegisterAsync(newUser);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, newUserData.Id.ToString()),
                new Claim(ClaimTypes.Name, newUserData.UserName),
                new Claim(ClaimTypes.Email, newUserData.Email)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType3");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            string eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001").ToString();

            var result = await _controller.GetEventParticipantsAsync(eventId);
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task GetEventParticipant_ShouldReturnOkResult_WhenUserIsEventParticipant()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.Parse("F0E9D8C7-B6A5-4321-FEDC-BA9876543210").ToString()),
                new Claim(ClaimTypes.Name, "Heisenberg"),
                new Claim(ClaimTypes.Email, "heisenberg@example.com")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType2");

            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            string eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001").ToString();
            var result = await _controller.GetEventParticipantsAsync(eventId);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult?.Value, Is.InstanceOf<IEnumerable<UserDto>>());
            var participants = okResult?.Value as IEnumerable<UserDto>;
            Assert.That(participants, Is.Not.Empty);
            Assert.That(participants.Count(), Is.EqualTo(3));

            Assert.That(participants.Any(p => p.Id == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(participants.Any(p => p.Id == "F0E9D8C7-B6A5-4321-FEDC-BA9876543210".ToLower()));
            Assert.That(participants.Any(p => p.Id == "11223344-5566-7788-99AA-BBCCDDEEFF00".ToLower()));

            Assert.That(participants.Any(p => p.UserName == "admin"));
            Assert.That(participants.Any(p => p.UserName == "Heisenberg"));
            Assert.That(participants.Any(p => p.UserName == "JessiePinkman"));

            Assert.That(participants.Any(p => p.Email == "admin@example.com"));
            Assert.That(participants.Any(p => p.Email == "heisenberg@example.com"));
            Assert.That(participants.Any(p => p.Email == "jessie@example.com"));
        }

        [Test]
        public async Task GetEventParticipant_ShouldReturnUnAuthorized_WhenUserIsNoAuthorized()
        {
            _controller.ControllerContext = new ControllerContext();

            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            string eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001").ToString();
            var result = await _controller.GetEventParticipantsAsync(eventId);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task GetEventParticipants_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "Mohamed"),
                new Claim(ClaimTypes.Email, "mohamed@example.com")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");

            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            string eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001").ToString();

            var result = await _controller.GetEventParticipantsAsync(eventId);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }
        #endregion

        #region AddEventParticipantAsync
        [Test]
        public async Task AddEventParticipantAsync_ShouldReturnOk_WhenParticipantAddedSuccessfully()
        {
            string eventId = "E1000000-0000-0000-0000-000000000001".ToLower();

            var newUser = new LoginAndRegisterInputDto()
            {
                Email = "ivan@example.com",
                Password = "password123",
                UserName = "Ivan"
            };

            var newUserData = await _userService.RegisterAsync(newUser);

            var result = await _controller.AddEventParticipant(eventId, newUserData.Id);

            Assert.That(result, Is.InstanceOf<NoContentResult>());

            // Verify that the participant was added
            bool isParticipant =
                await _eventParticipantsService.IsUserEventParticipant(Guid.Parse(newUserData.Id), Guid.Parse(eventId));

            Assert.That(isParticipant, Is.True, "The user should be added as a participant to the event.");
        }

        [Test]
        public async Task AddEventParticipantAsync_ShouldReturnBadRequest_WhenEventIdIsInvalid()
        {
            string eventId = "invalid id".ToLower();

            var newUser = new LoginAndRegisterInputDto()
            {
                Email = "ivan@example.com",
                Password = "password123",
                UserName = "Ivan"
            };

            var newUserData = await _userService.RegisterAsync(newUser);

            var result = await _controller.AddEventParticipant(eventId, newUserData.Id);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("Invalid eventId format."));
        }

        [Test]
        public async Task AddEventParticipantAsync_ShouldReturnBadRequest_WhenParticipantIdIsInvalid()
        {
            string eventId = "E1000000-0000-0000-0000-000000000001".ToLower();
            string participantId = "invalid id";

            var result = await _controller.AddEventParticipant(eventId, participantId);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("Invalid participantId format."));
        }

        [Test]
        public async Task AddEventParticipantAsync_ShouldReturnUnAuthorized_IfUserIsUnauthorized()
        {
            string eventId = "E1000000-0000-0000-0000-000000000001".ToLower();
            string participantId = "F0E9D8C7-B6A5-4321-FEDC-BA9876543210".ToLower(); // Heisenberg
            _controller.ControllerContext = new ControllerContext();
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            var result = await _controller.AddEventParticipant(eventId, participantId);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult?.Value, Is.EqualTo("You must be logged in to add a participant."));
        }

        [Test]
        public async Task AddEventParticipantAsync_ShouldReturnNotFound_WhenCurrentUserDoesNotExist()
        {
            var userId = Guid.NewGuid().ToString();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "Mohamed"),
                new Claim(ClaimTypes.Email, "mohamed@example.com")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");

            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            string eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001").ToString();

            var result = await _controller.AddEventParticipant(eventId, userId);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());

            var resultObjectResult = result as NotFoundObjectResult;
            Assert.That(resultObjectResult?.Value, Is.EqualTo($"User with ID {userId} not found."));
        }

        [Test]
        public async Task AddParticipantAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
        {
            string eventId = Guid.NewGuid().ToString();

            var newUser = new LoginAndRegisterInputDto()
            {
                Email = "example.com",
                Password = "password123",
                UserName = "Ivan"
            };

            var newUserData = await _userService.RegisterAsync(newUser);

            string participantId = newUserData.Id;

            var result = await _controller.AddEventParticipant(eventId, participantId);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult?.Value, Is.EqualTo($"Event with ID {eventId} not found."));
        }

        [Test]
        public async Task AddEventParticipantsAsync_ShouldNotFound_WhenParticipantDoesNotExist()
        {
            string eventId = "E1000000-0000-0000-0000-000000000001".ToLower();
            string participantId = Guid.NewGuid().ToString(); // Non-existent user ID

            var result = await _controller.AddEventParticipant(eventId, participantId);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult?.Value, Is.EqualTo($"Participant with ID {participantId} not found."));
        }

        [Test]
        public async Task AddEventParticipantAsync_ShouldReturnBadRequest_WhenParticipantAlreadyExists()
        {
            string eventId = "E1000000-0000-0000-0000-000000000001".ToLower();
            string participantId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower(); // admin
            var result = await _controller.AddEventParticipant(eventId, participantId);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo($"User with ID {participantId} is already a participant in event {eventId}."));
        }

        [Test]
        public async Task AddEventParticipantAsync_ShouldReturnForbidden_IfCurrentUserIsNotEventCreator()
        {
            var newUser = new LoginAndRegisterInputDto()
            {
                UserName    = "Mohamed",
                Email = "mohamed@example.com",
                Password = "password123"
            };

            var newUserData = await _userService.RegisterAsync(newUser);

            var userId = newUserData.Id;

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, newUserData.UserName),
                new Claim(ClaimTypes.Email, newUserData.Email)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");

            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            string eventId = "E1000000-0000-0000-0000-000000000001".ToLower();

            var result = await _controller.AddEventParticipant(eventId, userId);
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }
        #endregion

        #region RemoveParticipantAsync



        #endregion
    }
}
