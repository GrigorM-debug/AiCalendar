using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Controllers;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AiCalendar.Tests
{
    public class EventControllerTests : InMemoryDbTestBase
    {
        private EventController _eventController;
        private ILogger<EventController> _logger;
        private IEventService _eventService;
        private IRepository<Event> _eventRepository;
        private IUserService _userService;
        private IRepository<User> _userRepository;
        private IRepository<Participant> _participantRepository;


        [SetUp]
        public async Task Setup()
        {
            await Init();
            _eventRepository = new Repository<Event>(_context);
            _eventService = new EventService(_eventRepository);
            _logger = new Logger<EventController>(new LoggerFactory());
            _userRepository = new Repository<User>(_context);
            _participantRepository = new Repository<Participant>(_context);
            _userService = new UserService(_userRepository, _passwordHasher, _eventRepository, _participantRepository);
            _eventController = new EventController(_logger, _eventService, _userService);

            var userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"; // Admin user

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TearDown]
        public async Task TearDown()
        {
            await Dispose();
        }

        #region GetEventByIdAsync
        [Test]
        public async Task GetEventByIdAsync_ShouldReturnEvent_WhenEventExists()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001";

            // Act
            var result = await _eventController.GetEventByIdAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());

            var e = okResult.Value as EventDto;

            Assert.That(e, Is.Not.Null);
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
        }

        [Test]
        public async Task GetEventByIdAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000999"; // Non-existent event ID
            // Act
            var result = await _eventController.GetEventByIdAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("Event not found."));
        }

        [Test]
        public async Task GetEventByIdAsync_ShouldReturnBadRequest_WhenEventIdIsInvalid()
        {
            // Arrange
            var eventId = "InvalidEventId"; // Invalid event ID
            // Act
            var result = await _eventController.GetEventByIdAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
        #endregion

        #region CreateEventAsync

        [Test]
        public async Task CreateEventAsync_ShouldReturnCreatedEvent_WhenEventIsValid()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "New Event",
                Description = "This is a new event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };

            // Act
            var result = await _eventController.CreateEventAsync(createEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var createdResult = result as ObjectResult;

            Assert.That(createdResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

            Assert.That(createdResult.Value, Is.Not.Null);
            Assert.That(createdResult.Value, Is.InstanceOf<EventDto>());
            var e = createdResult.Value as EventDto;

            Assert.That(e.Id, Is.Not.Null.Or.Empty);
            Assert.That(e.Title, Is.EqualTo(createEventDto.Title));
            Assert.That(e.StartDate, Is.EqualTo(createEventDto.StartTime));
            Assert.That(e.EndDate, Is.EqualTo(createEventDto.EndTime));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo(createEventDto.Description));
        }

        [Test]
        public async Task CreateEventAsync_ShouldReturnUnAuthorized_WhenUserIsUnAuthorized()
        {
            _eventController.ControllerContext = new ControllerContext();
            _eventController.ControllerContext.HttpContext = new DefaultHttpContext();
            _eventController.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "New Event",
                Description = "This is a new event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };

            // Act
            var result = await _eventController.CreateEventAsync(createEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You must be logged in to create an event."));
        }

        [Test]
        public async Task CreateEventAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid().ToString(); 

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "ivan"),
                new Claim(ClaimTypes.Email, "ivan@example@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "New Event",
                Description = "This is a new event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };

            // Act
            var result = await _eventController.CreateEventAsync(createEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task CreateEventAsync_ShouldReturnConflict_WhenUserHasOverlappingEvents()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "New Event",
                Description = "This is a new event.",
                StartTime = new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc),
            };

            // Act
            var result = await _eventController.CreateEventAsync(createEventDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
            var conflictResult = result as ConflictObjectResult;
            Assert.That(conflictResult.Value, Is.EqualTo("You already have an event scheduled for this time period"));
        }

        #endregion

        #region DeleteEventAsync

        [Test]
        public async Task DeleteEventAsync_ShouldReturnUnAuthorized_WhenUserIsUnAuthorized()
        {
            _eventController.ControllerContext = new ControllerContext();
            _eventController.ControllerContext.HttpContext = new DefaultHttpContext();
            _eventController.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID

            // Act
            var result = await _eventController.DeleteEvent(eventId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You must be logged in to delete an event."));
        }

        [Test]
        public async Task DeleteEvent_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid().ToString(); 

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "pesho"),
                new Claim(ClaimTypes.Email, "pesho@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            // Act
            var result = await _eventController.DeleteEvent(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task DeleteEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
        {
            var userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"; // Admin user

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000999"; // Non-existent event ID
            // Act
            var result = await _eventController.DeleteEvent(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("Event not found."));
        }

        [Test]
        public async Task DeleteEvent_ShouldReturnBadRequest_WhenEventIdIsNotValidGuid()
        {
            var userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"; // Admin user

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "InvalidEventId"; // Invalid event ID
            // Act
            var result = await _eventController.DeleteEvent(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid event ID format."));
        }

        [Test]
        public async Task DeleteEvent_ShouldReturnForbidden_WhenUserIsNotEventCreator()
        {
            var userId = "F0E9D8C7-B6A5-4321-FEDC-BA9876543210"; 

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "Heisenberg"),
                new Claim(ClaimTypes.Email, "heisenberg@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Event created by another user
            // Act
            var result = await _eventController.DeleteEvent(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task DeleteEvent_ShouldReturnNoContent_WhenEventIsDeletedSuccessfully()
        {
            var userId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"; // Admin user

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            // Act
            var result = await _eventController.DeleteEvent(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var noContentResult = result as NoContentResult;
            Assert.That(noContentResult.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));

            // Verify that the event was deleted
            var eventExists = await _eventService.EventExistsByIdAsync(Guid.Parse(eventId));
            Assert.That(eventExists, Is.False, "Event should be deleted.");
        }

        #endregion

        #region CancelEventAsync   

        [Test]
        public async Task CancelEventAsync_ShouldReturnUnAuthorized_WhenUserIsUnAuthorized()
        {
            _eventController.ControllerContext = new ControllerContext();
            _eventController.ControllerContext.HttpContext = new DefaultHttpContext();
            _eventController.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            // Act
            var result = await _eventController.CancelEventAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You must be logged in to cancel an event."));
        }

        [Test]
        public async Task CancelEventAsync_ShouldReturnBadRequest_WhenEventIdIsInvalidGuid()
        {
            // Arrange
            var eventId = "InvalidEventId"; // Invalid event ID
            // Act
            var result = await _eventController.CancelEventAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid event ID format."));
        }

        [Test]
        public async Task CancelEventAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid().ToString(); 

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "nonexisting"),
                new Claim(ClaimTypes.Email, "nonexisting@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            // Act
            var result = await _eventController.CancelEventAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task CancelEventAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000999"; // Non-existent event ID
            // Act
            var result = await _eventController.CancelEventAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("Event not found."));
        }

        [Test]
        public async Task CancelEventAsync_ShouldReturnForbidden_WhenUserIsNotEventCreator()
        {
            var userId = "F0E9D8C7-B6A5-4321-FEDC-BA9876543210";
            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "Heisenberg"),
                new Claim(ClaimTypes.Email, "heisenberg@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Event created by another user
            // Act
            var result = await _eventController.CancelEventAsync(eventId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task CancelEventAsync_ShouldReturnNoContent_WhenEventIsCancelledSuccessfully()
        {
            var result = await _eventController.CancelEventAsync("E1000000-0000-0000-0000-000000000001");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());

            var e = okResult.Value as EventDto;
            Assert.That(e, Is.Not.Null);
            Assert.That(e.Id, Is.EqualTo("E1000000-0000-0000-0000-000000000001".ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
            Assert.That(e.IsCancelled, Is.True, "Event should be marked as cancelled.");
        }

        #endregion

        #region UpdateEventAsync

        [Test]
        public async Task UpdateEventAsync_ShouldReturnUnAuthorized_WhenUserIsUnAuthorized()
        {
            _eventController.ControllerContext = new ControllerContext();
            _eventController.ControllerContext.HttpContext = new DefaultHttpContext();
            _eventController.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("You must be logged in to update an event."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldReturnBadRequest_WhenEventIdIsInvalidGuid()
        {
            // Arrange
            var eventId = "InvalidEventId"; // Invalid event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid event ID format."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid().ToString();
            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "nonexisting"),
                new Claim(ClaimTypes.Email, "nonexisting@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID

            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };

            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000999"; // Non-existent event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("Event not found."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldReturnForbidden_WhenUserIsNotEventCreator()
        {
            var userId = "F0E9D8C7-B6A5-4321-FEDC-BA9876543210"; // Heisenberg user
            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "Heisenberg"),
                new Claim(ClaimTypes.Email, "heisenberg@example.com"),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claim, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _eventController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Event created by another user
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ForbidResult>());
            var forbidResult = result as ForbidResult;
            Assert.That(forbidResult, Is.Not.Null);
        }

        [Test]
        public async Task UpdateEventAsync_ShouldReturnConflict_WhenUserHasOverlappingEvents()
        {
            var eventId = "E1000000-0000-0000-0000-000000000001"; 
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc),
            };

            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
            var conflictResult = result as ConflictObjectResult;
            Assert.That(conflictResult.Value, Is.EqualTo("You already have an event scheduled for this time period"));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldReturnOk_WhenEventIsUpdatedSuccessfully()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(e.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(e.EndDate, Is.EqualTo(updateEventDto.EndTime));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo(updateEventDto.Description));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheTitle_WhenOnlyTheTitleIsProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
            };

            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;

            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheDescription_WhenOnlyTheDescriptionIsProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Description = "Updated description for the event.",
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo(updateEventDto.Description));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheStartAndEndTime_WhenOnlyTheTimesAreProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(e.EndDate, Is.EqualTo(updateEventDto.EndTime));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyStartDate_WhenStartDateIsProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyEndDate_WhenEndDateIsProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                EndTime = new DateTime(2025, 6, 16, 10, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(updateEventDto.EndTime));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTitleAndDescription_WhenTheyAreProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This is an updated event.",
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo(updateEventDto.Description));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTitleAndStartDate_WhenTheyAreProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(e.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTitleAndEndDate_WhenTheyAreProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(updateEventDto.EndTime));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo("Daily team synchronization meeting."));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyDescriptionAndStartDate_WhenTheyAreProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Description = "This is an updated event.",
                StartTime = new DateTime(2025, 6, 17, 10, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo(updateEventDto.Description));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyDescriptionAndEndDate_WhenTheyAreProvided()
        {
            // Arrange
            var eventId = "E1000000-0000-0000-0000-000000000001"; // Existing event ID
            var updateEventDto = new UpdateEventDto
            {
                Description = "This is an updated event.",
                EndTime = new DateTime(2025, 6, 17, 11, 0, 0, DateTimeKind.Utc),
            };
            // Act
            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Not.Null);
            Assert.That(okResult.Value, Is.InstanceOf<EventDto>());
            var e = okResult.Value as EventDto;
            Assert.That(e.Id, Is.EqualTo(eventId.ToLower()));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(updateEventDto.EndTime));
            Assert.That(e.CreatorId, Is.EqualTo("A1B2C3D4-E5F6-7890-1234-567890ABCDEF".ToLower()));
            Assert.That(e.Description, Is.EqualTo(updateEventDto.Description));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldReturnConflict_WhenTheyAreOverlappingEvents_WhenOnlyStartDateAndEndDateAreProvided()
        {
            var eventId = "E1000000-0000-0000-0000-000000000001";
            var updateEventDto = new UpdateEventDto
            {
                StartTime = new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc),
            };

            var result = await _eventController.UpdateEventAsync(eventId, updateEventDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
            var conflictResult = result as ConflictObjectResult;
            Assert.That(conflictResult.Value, Is.EqualTo("You already have an event scheduled for this time period"));
        }

        #endregion

        #region GetEvents

        

        #endregion
    }
}
