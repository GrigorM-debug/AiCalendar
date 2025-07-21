using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.EventParticipants;
using AiCalendar.WebApi.Services.EventParticipants.Interfaces;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;

namespace AiCalendar.Tests
{
    public class EventParticipantsServiceTests : InMemoryDbTestBase
    {
        private IRepository<Participant> _participantRepository;
        private IEventParticipantsService _eventParticipantsService;
        private IEventService _eventService;
        private IRepository<Event> _eventRepository;

        [SetUp]
        public async Task Setup()
        {
            await Init();
            _eventRepository = new Repository<Event>(_context);
            _participantRepository = new Repository<Participant>(_context);
            _eventParticipantsService = new EventParticipantsService(_participantRepository);
            _eventService = new EventService(_eventRepository);
        }

        [TearDown]
        public async Task TearDown()
        {
            // Clean up the in-memory database after each test
            await Dispose();
        }

        [Test]
        [TestCase("A1B2C3D4-E5F6-7890-1234-567890ABCDEF")]
        [TestCase("F0E9D8C7-B6A5-4321-FEDC-BA9876543210")]
        [TestCase("11223344-5566-7788-99AA-BBCCDDEEFF00")]
        public async Task IsUserEventParticipant_ShouldReturnTrue_IfUserIsEventParticipant(string userId)
        {
            Guid userIdGuid = Guid.Parse(userId);
            Guid event1Id = Guid.Parse("E1000000-0000-0000-0000-000000000001");

            bool isParticipant = await _eventParticipantsService.IsUserEventParticipant(userIdGuid, event1Id);

            Assert.That(isParticipant, Is.True);
        }

        [Test]
        public async Task IsUserEventParticipant_ShouldReturnFalse_IfUserIsNotEventParticipant()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            Guid event3Id = Guid.Parse("E1000000-0000-0000-0000-000000000003");
            bool isParticipant = await _eventParticipantsService.IsUserEventParticipant(userId, event3Id);
            Assert.That(isParticipant, Is.False);
        }

        [Test]
        public async Task GetParticipantsByEventIdAsync_ShouldReturnParticipants_ForValidEventId()
        {
            Guid event1Id = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            IEnumerable<UserDto> participants = await _eventParticipantsService.GetParticipantsByEventIdAsync(event1Id);
            Assert.That(participants, Is.Not.Null);
            Assert.That(participants.Count(), Is.EqualTo(3));

            Assert.That(participants.Any(p => p.Id.ToUpper() == "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"),
                Is.True); // admin
            Assert.That(participants.Any(p => p.Id.ToUpper() == "F0E9D8C7-B6A5-4321-FEDC-BA9876543210"),
                Is.True); // Heisenberg
            Assert.That(participants.Any(p => p.Id.ToUpper() == "11223344-5566-7788-99AA-BBCCDDEEFF00"),
                Is.True); // JessiePinkman

            Assert.That(participants.Any(p => p.UserName == "admin"), Is.True);
            Assert.That(participants.Any(p => p.UserName == "Heisenberg"), Is.True);
            Assert.That(participants.Any(p => p.UserName == "JessiePinkman"), Is.True);

            Assert.That(participants.Any(p => p.Email == "admin@example.com"), Is.True);
            Assert.That(participants.Any(p => p.Email == "heisenberg@example.com"), Is.True);

            Assert.That(participants.Any(p => p.Email == "jessie@example.com"), Is.True);
        }

        [Test]
        public async Task GetParticipantsByEventId_ShouldReturnEmptyCollection_IfThereAreNoParticipants()
        {
            var newEvent = new CreateEventDto()
            {
                Title = "New Event",
                Description = "This is a new event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var createdEvent =
                await _eventService.CreateEventAsync(newEvent, Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"));

            IEnumerable<UserDto> participants =
                await _eventParticipantsService.GetParticipantsByEventIdAsync(Guid.Parse(createdEvent.Id));

            Assert.That(participants, Is.Not.Null);
        }

        [Test]
        public async Task AddParticipantAsync_ShouldAddParticipant_ToEvent()
        {
            var newEvent = new CreateEventDto()
            {
                Title = "New Event",
                Description = "This is a new event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var createdEvent =
                await _eventService.CreateEventAsync(newEvent, Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF"));

            Guid participantId = Guid.Parse("F0E9D8C7-B6A5-4321-FEDC-BA9876543210");
            Guid eventId = Guid.Parse(createdEvent.Id);

            await _eventParticipantsService.AddParticipantAsync(participantId, eventId);
            bool isParticipant = await _eventParticipantsService.IsUserEventParticipant(participantId, eventId);
            Assert.That(isParticipant, Is.True);
        }

        [Test]
        public async Task RemoveParticipantAsync_ShouldRemoveParticipant_FromEvent()
        {
            Guid participantId = Guid.Parse("F0E9D8C7-B6A5-4321-FEDC-BA9876543210");
            Guid eventId = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            await _eventParticipantsService.RemoveParticipantAsync(participantId, eventId);
            bool isParticipant = await _eventParticipantsService.IsUserEventParticipant(participantId, eventId);
            Assert.That(isParticipant, Is.False);
        }
    }
}
