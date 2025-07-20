using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;

namespace AiCalendar.Tests
{
    public class EventServiceTests : InMemoryDbTestBase
    {
        private IEventService _eventService;
        private IRepository<Event> _eventRepository;

        [SetUp]
        public async Task Setup()
        {
            await Init();
            _eventRepository = new Repository<Event>(_context);
            _eventService = new EventService(_eventRepository);
        }

        [TearDown]
        public async Task Cleanup()
        {
            await Dispose();
        }

        [Test]
        [TestCase("E1000000-0000-0000-0000-000000000001")]
        [TestCase("E1000000-0000-0000-0000-000000000002")]
        [TestCase("E1000000-0000-0000-0000-000000000003")]
        [TestCase("E1000000-0000-0000-0000-000000000004")]
        public async Task GetEventById_ShouldReturnEvent_WhenIdExists(string eventId)
        {
            // Arrange
            Guid id = Guid.Parse(eventId);
            // Act
            var result = await _eventService.GetEventByIdAsync(id);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(id.ToString()));
        }

        [Test]
        [TestCase("E1000000-0000-0000-0000-000000000005")]
        [TestCase("E1000000-0000-0000-0000-000000000006")]
        public async Task GetEventById_ShouldReturnNull_WhenIdDoesNotExist(string eventId)
        {
            // Arrange
            Guid id = Guid.Parse(eventId);
            // Act
            var result = await _eventService.GetEventByIdAsync(id);
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase("E1000000-0000-0000-0000-000000000001")]
        [TestCase("E1000000-0000-0000-0000-000000000002")]
        [TestCase("E1000000-0000-0000-0000-000000000003")]
        [TestCase("E1000000-0000-0000-0000-000000000004")]
        public async Task EventExistsByIdAsync_ShouldReturnTrue_WhenIdExists(string eventId)
        {
            Guid id = Guid.Parse(eventId);

            var result = await _eventService.EventExistsByIdAsync(id);

            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("E1000000-0000-0000-0000-000000000005")]
        [TestCase("E1000000-0000-0000-0000-000000000006")]
        public async Task EventExistsByIdAsync_ShouldReturnFalse_WhenIdDoesNotExist(string eventId)
        {
            Guid id = Guid.Parse(eventId);
            var result = await _eventService.EventExistsByIdAsync(id);
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CreateEventAsync_ShouldCreateEvent_WhenValidDataProvided()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "New Event",
                Description = "This is a new event.",
                StartTime = new DateTime(2025, 6, 22, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 22, 12, 0, 0, DateTimeKind.Utc)
            };
            Guid creatorId = Guid.Parse("E1000000-0000-0000-0000-000000000001"); // Assuming this user exists
            // Act
            var createdEvent = await _eventService.CreateEventAsync(createEventDto, creatorId);
            // Assert
            Assert.That(createdEvent, Is.Not.Null);
            Assert.That(createdEvent.Title, Is.EqualTo(createEventDto.Title));
            Assert.That(createdEvent.Description, Is.EqualTo(createEventDto.Description));
            Assert.That(createdEvent.StartDate, Is.EqualTo(createEventDto.StartTime));
            Assert.That(createdEvent.EndDate, Is.EqualTo(createEventDto.EndTime));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnAllEvents_IfNoFilterIsApplied()
        {
            // Act
            var result = await _eventService.GetEventsAsync();
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(4)); // Assuming we have seeded 4 events

            Assert.That(result.Any(e => e.Title == "Team Stand-up Meeting"));
            Assert.That(result.Any(e => e.StartDate == new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.EndDate == new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.Description == "Daily team synchronization meeting."));

            Assert.That(result.Any(e => e.Title == "Project X Review"));
            Assert.That(result.Any(e => e.StartDate == new DateTime(2025, 6, 17, 14, 0, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.EndDate == new DateTime(2025, 6, 17, 15, 30, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.Description == "Review progress on Project X with stakeholders."));

            Assert.That(result.Any(e => e.Title == "Dentist Appointment"));
            Assert.That(result.Any(e => e.StartDate == new DateTime(2025, 6, 20, 8, 0, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.EndDate == new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.Description == "Routine check-up."));

            Assert.That(result.Any(e => e.Title == "Weekend Hike"));
            Assert.That(result.Any(e => e.StartDate == new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.EndDate == new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc)));
            Assert.That(result.Any(e => e.Description == "Exploring the Vitosha mountains."));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEmptyCollection_IfNoEventsExists()
        {
            await _context.Database.EnsureDeletedAsync();

            var result = await _eventService.GetEventsAsync();

            Assert.That(result, Is.Empty);
        }

    }
}
