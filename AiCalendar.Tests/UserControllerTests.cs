using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Controllers;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        }

        [TearDown]
        public async Task TearDown()
        {
            await Dispose();
        }
    }
}
