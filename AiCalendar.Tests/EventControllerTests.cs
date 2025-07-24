using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Controllers;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;
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

        [SetUp]
        public async Task Setup()
        {
            await Init();
            _eventRepository = new Repository<Event>(_context);
            _eventService = new EventService(_eventRepository);
            _logger = new Logger<EventController>(new LoggerFactory());
            _eventController = new EventController(_logger, _eventService);
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

            var e = result as EventDto;

            Assert.That(e, Is.Not.Null);
            Assert.That(e.Id, Is.EqualTo(eventId));
            Assert.That(e.Title, Is.EqualTo("Team Stand-up Meeting"));
            Assert.That(e.StartDate, Is.EqualTo(new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(e.EndDate, Is.EqualTo(new DateTime(2023, 10, 1, 11, 0, 0)));
            Assert.That(e.CreatorId, Is.EqualTo(new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
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
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
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
    }
}
