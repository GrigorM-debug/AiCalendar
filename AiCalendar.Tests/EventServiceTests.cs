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
using Microsoft.Identity.Client;

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

        #region GetEventById
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
        [TestCase("E1000000-0000-0000-0000-000000000008")]
        [TestCase("E1000000-0000-0000-0000-000000000009")]
        public async Task GetEventById_ShouldReturnNull_WhenIdDoesNotExist(string eventId)
        {
            // Arrange
            Guid id = Guid.Parse(eventId);
            // Act
            var result = await _eventService.GetEventByIdAsync(id);
            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion

        #region EventExistsByIdAsync
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
        [TestCase("E1000000-0000-0000-0000-000000000008")]
        [TestCase("E1000000-0000-0000-0000-000000000009")]
        public async Task EventExistsByIdAsync_ShouldReturnFalse_WhenIdDoesNotExist(string eventId)
        {
            Guid id = Guid.Parse(eventId);
            var result = await _eventService.EventExistsByIdAsync(id);
            Assert.That(result, Is.False);
        }

        #endregion

        #region CreateEventAsync
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
        #endregion

        #region GetAllEventsAsync
        [Test]
        public async Task GetAllEventsAsync_ShouldReturnAllEvents_IfNoFilterIsApplied()
        {
            // Act
            var result = await _eventService.GetEventsAsync();
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(6)); // Assuming we have seeded 4 events

            var resultToList = result.ToList();

            Assert.That(resultToList[0].Title == "Team Stand-up Meeting");
            Assert.That(resultToList[0].StartDate == new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[0].EndDate == new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[0].Description == "Daily team synchronization meeting.");
            Assert.That(resultToList[0].IsCancelled == false);

            Assert.That(resultToList[1].Title == "Project X Review");
            Assert.That(resultToList[1].StartDate == new DateTime(2025, 6, 17, 14, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[1].EndDate == new DateTime(2025, 6, 17, 15, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[1].Description == "Review progress on Project X with stakeholders.");
            Assert.That(resultToList[1].IsCancelled == false);

            Assert.That(resultToList[2].Title == "Dentist Appointment");
            Assert.That(resultToList[2].StartDate == new DateTime(2025, 6, 20, 8, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[2].EndDate == new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[2].Description == "Routine check-up.");
            Assert.That(resultToList[2].IsCancelled == false);

            Assert.That(resultToList[3].Title == "Weekend Hike");
            Assert.That(resultToList[3].StartDate == new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[3].EndDate == new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[3].Description == "Exploring the Vitosha mountains.");
            Assert.That(resultToList[3].IsCancelled == false);

            Assert.That(resultToList[4].Title == "Lunch with Sarah");
            Assert.That(resultToList[4].StartDate == new DateTime(2025, 6, 18, 12, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[4].EndDate == new DateTime(2025, 6, 18, 13, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[4].Description == "Catching up with Sarah at the new cafe.");
            Assert.That(resultToList[4].IsCancelled == true);

            Assert.That(resultToList[5].Title == "Code Review Session");
            Assert.That(resultToList[5].StartDate == new DateTime(2025, 6, 19, 10, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[5].EndDate == new DateTime(2025, 6, 19, 11, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[5].Description == "Reviewing code for the new feature implementation.");
            Assert.That(resultToList[5].IsCancelled == true);
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEmptyCollection_IfNoEventsExists()
        {
            await _context.Database.EnsureDeletedAsync();

            var result = await _eventService.GetEventsAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnFilteredEvents_WhenFilterIsApplied()
        {
            // Arrange
            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2025, 6, 16, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 6, 20, 23, 59, 59, DateTimeKind.Utc),
                IsCancelled = false
            };
            // Act
            var result = await _eventService.GetEventsAsync(filter);

            var resultToList = result.ToList();

            // Assert
            Assert.That(result, Is.Not.Empty);

            Assert.That(result.Count(),
                Is.EqualTo(3));

            Assert.That(resultToList[0].Title == "Team Stand-up Meeting");
            Assert.That(resultToList[0].Description == "Daily team synchronization meeting.");

            Assert.That(resultToList[1].Title == "Project X Review");
            Assert.That(resultToList[1].Description == "Review progress on Project X with stakeholders.");
            
            Assert.That(resultToList[2].Title == "Dentist Appointment");
            Assert.That(resultToList[2].Description == "Routine check-up.");

            Assert.That(result.All(e => e.IsCancelled == false));

            Assert.That(result.All(e => e.StartDate >= new DateTime(2025, 6, 16, 0, 0, 0, DateTimeKind.Utc)));

            Assert.That(result.All(e => e.EndDate <= new DateTime(2025, 6, 20, 23, 59, 59, DateTimeKind.Utc)));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEmptyCollection_WhenNoEventsMatchFilter()
        {
            // Arrange
            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2025, 6, 22, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 6, 22, 23, 59, 59, DateTimeKind.Utc)
            };
            // Act
            var result = await _eventService.GetEventsAsync(filter);
            // Assert
            Assert.That(result, Is.Empty);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEvents_MatchingTheStartDateFilter()
        {
            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)
            };
            var result = await _eventService.GetEventsAsync(filter);
            // Assert
            Assert.That(result, Is.Not.Empty);

            Assert.That(result.All(e => e.StartDate >= new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEmptyCollection_WhenNoEventsMatchStartDateFilter()
        {
            var filter = new EventFilterCriteriaDto()
            {
                StartDate = new DateTime(2025, 6, 23, 0, 0, 0, DateTimeKind.Utc)
            };
            var result = await _eventService.GetEventsAsync(filter);
            // Assert
            Assert.That(result, Is.Empty);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEvents_MatchingTheEndDateFilter()
        {
            var filter = new EventFilterCriteriaDto()
            {
                EndDate = new DateTime(2025, 6, 22, 23, 59, 59, DateTimeKind.Utc)
            };

            var result = await _eventService.GetEventsAsync(filter);
            
            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.All(e => e.EndDate <= new DateTime(2025, 6, 22, 23, 59, 59, DateTimeKind.Utc)));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEmptyCollection_WhenNoEventsMatchEndDateFilter()
        {
            var filter = new EventFilterCriteriaDto()
            {
                EndDate = new DateTime(2020, 6, 21, 15, 0, 0, DateTimeKind.Utc)
            };
            var result = await _eventService.GetEventsAsync(filter);
            // Assert
            Assert.That(result, Is.Empty);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnOnlyCancelledEvents_WhenTheFilterIsApplied()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            await _eventService.CancelEventAsync(event4Id, user1Id);

            var filter = new EventFilterCriteriaDto()
            {
                IsCancelled = true
            };

            var result = await _eventService.GetEventsAsync(filter);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Count(), Is.EqualTo(3)); // Only one event should be cancelled

            var resultToList = result.ToList();

            Assert.That(resultToList.All(e => e.IsCancelled == true));

            Assert.That(resultToList[0].Title == "Weekend Hike");
            Assert.That(resultToList[0].Description == "Exploring the Vitosha mountains.");
            Assert.That(resultToList[0].StartDate == new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[0].EndDate == new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[0].IsCancelled == true);

            Assert.That(resultToList[1].Title == "Lunch with Sarah");
            Assert.That(resultToList[1].Description == "Catching up with Sarah at the new cafe.");
            Assert.That(resultToList[1].StartDate == new DateTime(2025, 6, 18, 12, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[1].EndDate == new DateTime(2025, 6, 18, 13, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[1].IsCancelled == true);

            Assert.That(resultToList[2].Title == "Code Review Session");
            Assert.That(resultToList[2].Description == "Reviewing code for the new feature implementation.");
            Assert.That(resultToList[2].StartDate == new DateTime(2025, 6, 19, 10, 0, 0, DateTimeKind.Utc));
            Assert.That(resultToList[2].EndDate == new DateTime(2025, 6, 19, 11, 30, 0, DateTimeKind.Utc));
            Assert.That(resultToList[2].IsCancelled == true);
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnEmptyCollection_WhenNoCancelledEventsExist()
        {
            await _eventService.UncancelEventAsync(Guid.Parse("E1000000-0000-0000-0000-000000000004"), Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"));

            await _eventService.UncancelEventAsync(Guid.Parse("E1000000-0000-0000-0000-000000000005"), Guid.Parse("F0E9D8C7-B6A5-4321-FEDC-BA9876543210"));

            await _eventService.UncancelEventAsync(Guid.Parse("E1000000-0000-0000-0000-000000000006"), Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"));

            var filter = new EventFilterCriteriaDto()
            {
                IsCancelled = true
            };

            var result = await _eventService.GetEventsAsync(filter);

            // Assert
            Assert.That(result, Is.Empty);
            Assert.That(result.Count(), Is.EqualTo(0));
        }
        #endregion

        #region DeleteEventAsync
        [Test]
        public async Task DeleteEventAsync_ShouldDeleteEvent_WhenIdExists()
        {
            // Arrange
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            // Act
            await _eventService.DeleteEventAsync(eventId);
            // Assert
            var deletedEvent = await _eventService.GetEventByIdAsync(eventId);
            Assert.That(deletedEvent, Is.Null);
        }
        #endregion

        #region IsUserEventCreator
        [Test]
        public async Task IsUserEventCreator_ShouldReturnTrue_IfUserIsEventCreator()
        {
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"); // Assuming this user is the creator
            bool isCreator = await _eventService.IsUserEventCreator(eventId, userId);
            Assert.That(isCreator, Is.True);
        }

        [Test]
        public async Task IsUserEventCreator_ShouldReturnFalse_IfUserIsNotEventCreator()
        {
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            Guid userId = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00"); 
            bool isCreator = await _eventService.IsUserEventCreator(eventId, userId);
            Assert.That(isCreator, Is.False);
        }
        #endregion

        #region CancelEventAsync
        [Test]
        public async Task CancelEventAsync_ShouldCancelEvent_IfItExists()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var result = await _eventService.CancelEventAsync(event4Id, user1Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsCancelled, Is.True);
            Assert.That(result.Id, Is.EqualTo(event4Id.ToString()));
            Assert.That(result.CreatorId, Is.EqualTo(user1Id.ToString()));
        }
        #endregion

        #region UpdateEventAsync
        [Test]
        public async Task UpdateEventAsync_ShouldUpdateEvent_WhenValidDataProvided()
        {
            // Arrange
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"); 
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Event",
                Description = "This event has been updated.",
                StartTime = new DateTime(2025, 6, 22, 11, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 6, 22, 13, 0, 0, DateTimeKind.Utc)
            };
            // Act
            var updatedEvent = await _eventService.UpdateEvent(eventId, updateEventDto, userId);
            // Assert
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(updatedEvent.Description, Is.EqualTo(updateEventDto.Description));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(updateEventDto.EndTime));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheTitle_WhenOnlyTheTitleIsProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Weekend Hike"
            };

            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);

            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(updatedEvent.Description, Is.EqualTo("Exploring the Vitosha mountains."));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc)));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheDescription_WhenOnlyTheDescriptionIsProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var updateEventDto = new UpdateEventDto
            {
                Description = "Exploring the Vitosha mountains. Updated"
            };

            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);

            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo("Weekend Hike"));
            Assert.That(updatedEvent.Description, Is.EqualTo(updateEventDto.Description));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc)));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheStartTime_WhenOnlyTheStartTimeIsProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updateEventDto = new UpdateEventDto
            {
                StartTime = new DateTime(2025, 6, 21, 8, 0, 0, DateTimeKind.Utc)
            };
            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo("Weekend Hike"));
            Assert.That(updatedEvent.Description, Is.EqualTo("Exploring the Vitosha mountains."));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheEndDate_WhenOnlyTheEndDateIsProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updateEventDto = new UpdateEventDto
            {
                EndTime = new DateTime(2026, 7, 28, 5, 0, 0, DateTimeKind.Utc)
            };
            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo("Weekend Hike"));
            Assert.That(updatedEvent.Description, Is.EqualTo("Exploring the Vitosha mountains."));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc)));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(updateEventDto.EndTime));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheTitleAndDescription_WhenOnlyTheyAreProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Weekend Hike",
                Description = "Exploring the Vitosha mountains. Updated"
            };
            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(updatedEvent.Description, Is.EqualTo(updateEventDto.Description));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc)));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheTitleAndTheStartDate_WhenOnlyTheyAreProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Weekend Hike",
                StartTime = new DateTime(2025, 6, 21, 8, 0, 0, DateTimeKind.Utc)
            };

            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(updatedEvent.Description, Is.EqualTo("Exploring the Vitosha mountains."));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheTitleAndTheEndDate_WhenOnlyTheyAreProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Weekend Hike",
                EndTime = new DateTime(2026, 7, 28, 5, 0, 0, DateTimeKind.Utc)
            };
            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo(updateEventDto.Title));
            Assert.That(updatedEvent.Description, Is.EqualTo("Exploring the Vitosha mountains."));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc)));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(updateEventDto.EndTime));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheDescriptionAndStartDate_WhenOnlyTheyAreProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updateEventDto = new UpdateEventDto
            {
                Description = "Exploring the Vitosha mountains. Updated",
                StartTime = new DateTime(2025, 6, 21, 8, 0, 0, DateTimeKind.Utc)
            };

            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo("Weekend Hike"));
            Assert.That(updatedEvent.Description, Is.EqualTo(updateEventDto.Description));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(updateEventDto.StartTime));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public async Task UpdateEventAsync_ShouldUpdateOnlyTheDescriptionAndEndDate_WhenOnlyTheyAreProvided()
        {
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updateEventDto = new UpdateEventDto
            {
                Description = "Exploring the Vitosha mountains. Updated",
                EndTime = new DateTime(2026, 7, 28, 5, 0, 0, DateTimeKind.Utc)
            };
            var updatedEvent = await _eventService.UpdateEvent(event4Id, updateEventDto, user1Id);
            Assert.That(updatedEvent, Is.Not.Null);
            Assert.That(updatedEvent.Title, Is.EqualTo("Weekend Hike"));
            Assert.That(updatedEvent.Description, Is.EqualTo(updateEventDto.Description));
            Assert.That(updatedEvent.StartDate, Is.EqualTo(new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc)));
            Assert.That(updatedEvent.EndDate, Is.EqualTo(updateEventDto.EndTime));
        }
        #endregion

        #region HasOverlappingEvents
        [Test]
        public async Task HasOverlappingEvents_ShouldReturnTrue_WhenUserHasOverlappingEvents()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            DateTime startTime = new DateTime(2025, 6, 16, 9, 15, 0, DateTimeKind.Utc);
            DateTime endTime = new DateTime(2025, 6, 16, 9, 45, 0, DateTimeKind.Utc);
            bool hasOverlapping = await _eventService.HasOverlappingEvents(userId, startTime, endTime);
            Assert.That(hasOverlapping, Is.True);
        }

        [Test]
        public async Task HasOverlappingEvents_ShouldReturnFalse_WhenUserHasNoOverlappingEvents()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            DateTime startTime = new DateTime(2025, 6, 22, 10, 0, 0, DateTimeKind.Utc);
            DateTime endTime = new DateTime(2025, 6, 22, 12, 0, 0, DateTimeKind.Utc);
            bool hasOverlapping = await _eventService.HasOverlappingEvents(userId, startTime, endTime);
            Assert.That(hasOverlapping, Is.False);
        }
        #endregion

        #region HasOverlappingEventsExcludingTheCurrentEvent
        [Test]
        public async Task HasOverlappingEventsExcludingTheCurrentEvent_ShouldReturnTrue_WhenUserhasOverlappingEvents()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            Guid currentEventId = Guid.Parse("E1000000-0000-0000-0000-000000000003");
            DateTime startTime = new DateTime(2025, 6, 16, 9, 15, 0, DateTimeKind.Utc);
            DateTime endTime = new DateTime(2025, 6, 16, 9, 45, 0, DateTimeKind.Utc);

            bool hasOverlapping = await _eventService.HasOverlappingEventsExcludingTheCurrentEvent(userId, startTime, endTime, currentEventId);

            Assert.That(hasOverlapping, Is.True);
        }

        [Test]
        public async Task HasOverlappingEventsExcludingTheCurrentEvent_ShouldReturnFalse_WhenUserHasNoOverlappingEvents()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            Guid currentEventId = Guid.Parse("E1000000-0000-0000-0000-000000000003");
            DateTime startTime = new DateTime(2025, 6, 22, 10, 0, 0, DateTimeKind.Utc);
            DateTime endTime = new DateTime(2025, 6, 22, 12, 0, 0, DateTimeKind.Utc);
            bool hasOverlapping = await _eventService.HasOverlappingEventsExcludingTheCurrentEvent(userId, startTime, endTime, currentEventId);
            Assert.That(hasOverlapping, Is.False);
        }
        #endregion

        #region CheckIfEventIsAlreadyCancelled
        [Test]
        public async Task CheckIfEventIsAlreadyCancelled_ShouldReturnTrue_WhenEventIsCancelled()
        {
            await _eventService.CancelEventAsync(Guid.Parse("E1000000-0000-0000-0000-000000000001"),
                Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"));

            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            bool isCancelled = await _eventService.CheckIfEventIsAlreadyCancelled(eventId);
            Assert.That(isCancelled, Is.True);
        }

        [Test]
        public async Task CheckIfEventIsAlreadyCancelled_ShouldReturnFalse_WhenEventIsNotCancelled()
        {
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            bool isCancelled = await _eventService.CheckIfEventIsAlreadyCancelled(eventId);
            Assert.That(isCancelled, Is.False);
        }
        #endregion

        #region CheckIfEventIsAlreadyUnCancelled

        [Test]
        public async Task CheckIfEventIsAlreadyUnCancelled_ShouldReturnFalse_WhenEventIsActive()
        {
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            bool isCancelled = await _eventService.CheckIfEventIsAlreadyUncanceled(eventId);
            Assert.That(isCancelled, Is.True);
        }

        [Test]
        public async Task CheckIfEventIsAlreadyUnCancelled_ShouldReturnTrue_WhenEventIsCancelled()
        {
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000005");
            bool isCancelled = await _eventService.CheckIfEventIsAlreadyUncanceled(eventId);
            Assert.That(isCancelled, Is.False);
        }

        #endregion

        #region CheckIfEventExistsByTitleAndDescription

        [Test]
        public async Task CheckIfEventExistsByTitleAndDescription_ShouldReturnTrue_IfEventWithTitleAlreadyExists()
        {
            string title = "Dentist Appointment";
            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");


            bool result = await _eventService.CheckIfEventExistsByTitleAndDescription(title, null, user3Id);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckIfEventExistsByTitleAndDescription_ShouldReturnFalse_IfEventWithTitleDoesNotExists()
        {
            string title = "Random Title Event";
            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");

            bool result = await _eventService.CheckIfEventExistsByTitleAndDescription(title, null, user3Id);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CheckIfEventExistsByTitleAndDescription_ShouldReturnTrue_IfEventWithDescriptionExists()
        {
            string description = "Routine check-up.";
            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");


            bool result = await _eventService.CheckIfEventExistsByTitleAndDescription(null, description, user3Id);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckIfEventExistsByTitleAndDescription_ShouldReturnFalse_IfEventWithDescriptionDoesNotExists()
        {
            string description = "Random Description Event";
            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");

            bool result = await _eventService.CheckIfEventExistsByTitleAndDescription(null, description, user3Id);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CheckIfEventExistsByTitleAndDescription_ShouldReturnTrue_IfEventWithTitleAndDescriptionExists()
        {
            string title = "Dentist Appointment";

            string description = "Routine check-up.";

            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");

            bool result = await _eventService.CheckIfEventExistsByTitleAndDescription(title, description, user3Id);
            
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckIfEventExistsByTitleAndDescription_ShouldReturnFalse_IfEventWithTitleAndDescriptionDoesNotExists()
        {
            string title = "Random Title Event";
            
            string description = "Random Description Event";

            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");

            bool result = await _eventService.CheckIfEventExistsByTitleAndDescription(title, description, user3Id);
            
            Assert.That(result, Is.False);
        }

        #endregion
    }
}
