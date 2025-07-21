using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.EventParticipants;
using AiCalendar.WebApi.Services.EventParticipants.Interfaces;

namespace AiCalendar.Tests
{
    public class EventParticipantsServiceTests : InMemoryDbTestBase
    {
        private IRepository<Participant> _participantRepository;
        private IEventParticipantsService _eventParticipantsService;

        [SetUp]
        public async Task Setup()
        {
            await Init();
            _participantRepository = new Repository<Participant>(_context);
            _eventParticipantsService = new EventParticipantsService(_participantRepository);
        }

        [TearDown]
        public async Task TearDown()
        {
            // Clean up the in-memory database after each test
            await Dispose();
        }
    }
}
