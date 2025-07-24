using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Controllers;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;
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
    }
}
