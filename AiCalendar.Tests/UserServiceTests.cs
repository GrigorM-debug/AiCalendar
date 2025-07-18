using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;

namespace AiCalendar.Tests
{
    [TestFixture]
    public class UserServiceTests : InMemoryDbTestBase
    {
        private IRepository<User> _userRepository;
        private IRepository<Event> _eventRepository;
        private IRepository<Participant> _participantRepository;
        private IUserService _userService;
        [SetUp]
        public async Task Setup()
        {
            await Init();
            _userRepository = new Repository<User>(_context);
            _eventRepository = new Repository<Event>(_context);
            _participantRepository = new Repository<Participant>(_context);
            _userService = new UserService(_userRepository, _passwordHasher, _eventRepository, _participantRepository);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Dispose();
        }

        [Test]
        [TestCase("JessiePinkman")]
        [TestCase("Heisenberg")]
        [TestCase("admin")]
        public async Task UserExistsByUsernameAsyncShouldReturnTrueIfTheUserExists(string username)
        {
            bool isUserExists = await _userService.UserExistsByUsernameAsync(username);

            Assert.That(isUserExists, $"User with username {username} should exist.");
        }
    }
}
