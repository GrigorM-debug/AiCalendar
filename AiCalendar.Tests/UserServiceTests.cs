using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [Test]
        [TestCase("NonExistentUser")]
        [TestCase("AnotherNonExistentUser")]
        [TestCase("ThirdNonExistingUser")]
        public async Task UserExistsByUsernameAsyncShouldReturnFalseIfTheUserDoesNotExist(string username)
        {
            bool isUserExists = await _userService.UserExistsByUsernameAsync(username);
            Assert.That(isUserExists, Is.False, $"User with username {username} should not exist.");
        }

        [Test]
        public async Task GetUserByUsernameAsyncShouldReturnUserIfExists()
        {
            string username = "JessiePinkman";
            User? user = await _userService.GetUserByUsernameAsync(username);
            Assert.That(user, Is.Not.Null, $"User with username {username} should exist.");
            Assert.That(user?.UserName, Is.EqualTo(username), "Returned user does not match the requested username.");
        }

        [Test]
        public async Task GetUserByUsernameAsyncShouldReturnNullIfUserDoesNotExist()
        {
            string username = "NonExistentUser";
            User? user = await _userService.GetUserByUsernameAsync(username);
            Assert.That(user, Is.Null, $"User with username {username} should not exist.");
        }

        [Test]
        public async Task GetUserByIdAsyncShouldReturnUserIfExists()
        {
            Guid userId = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");
            User? user = await _userService.GetUserByIdAsync(userId);
            Assert.That(user, Is.Not.Null, $"User with ID {userId} should exist.");
            Assert.That(user?.Id, Is.EqualTo(userId), "Returned user does not match the requested ID.");
        }

        [Test]
        public async Task GetUserByIdAsyncShouldReturnNullIfUserDoesNotExist()
        {
            Guid userId = Guid.NewGuid(); // Random ID that does not exist
            User? user = await _userService.GetUserByIdAsync(userId);
            Assert.That(user, Is.Null, $"User with ID {userId} should not exist.");
        }

        [Test]
        public async Task UserExistsByIdAsyncShouldReturnTrueIfUserExists()
        {
            Guid userId = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");
            bool exists = await _userService.UserExistsByIdAsync(userId);
            Assert.That(exists, Is.True, $"User with ID {userId} should exist.");
        }

        [Test]
        public async Task UserExistsByIdAsyncShouldReturnFalseIfUserDoesNotExist()
        {
            Guid userId = Guid.NewGuid(); // Random ID that does not exist
            bool exists = await _userService.UserExistsByIdAsync(userId);
            Assert.That(exists, Is.False, $"User with ID {userId} should not exist.");
        }

        [Test]
        [TestCase("JessiePinkman", "jessie@example.com")]
        [TestCase("Heisenberg", "heisenberg@example.com")]
        [TestCase("admin", "admin@example.com")]
        public async Task GetUserByUserNameAndEmailShouldReturnUserIfExists(string username, string email)
        {
            User? user = await _userService.GetUserByUserNameAndEmail(username, email);

            Assert.That(user, Is.Not.Null, $"User with username {username} and email {email} should exist.");
            Assert.That(user?.UserName, Is.EqualTo(username), "Returned user does not match the requested username.");
            Assert.That(user?.Email, Is.EqualTo(email), "Returned user does not match the requested email.");
        }

        [Test]
        [TestCase("NonExistingUser", "nonexistinguser@example.com")]
        [TestCase("AnotherNonExistingUser", "anotherNonExistingUser@example.com")]
        public async Task GetUserByUserNameAndEmailShouldReturnNullIfUserDoesNotExist(string username, string email)
        {
            User? user = await _userService.GetUserByUserNameAndEmail(username, email);

            Assert.That(user, Is.Null, $"User with username {username} and email {email} should not exist.");
        }

        [Test]
        public async Task RegisterAsyncShouldCreateNewUser()
        {
            var input = new LoginAndRegisterInputDto
            {
                UserName = "NewUser",
                Email = "newUser@example.com",
                Password = "NewUserPassword123"
            };

            UserDto newUser = await _userService.RegisterAsync(input);

            Assert.That(newUser, Is.Not.Null, "New user should be created.");
            Assert.That(newUser.UserName, Is.EqualTo(input.UserName), "New user's username does not match the input.");
            Assert.That(newUser.Email, Is.EqualTo(input.Email), "New user's email does not match the input.");
        }

        [Test]
        public async Task CheckIfUserHasActiveEventsShouldReturnTrueIfUserHasActiveEvents()
        {
            Guid userId = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");

            bool hasActiveEvents = await _userService.CheckIfUserHasActiveEvents(userId);
            Assert.That(hasActiveEvents, Is.True, $"User with ID {userId} should have active events.");
        }

        [Test]
        public async Task CheckIfUserHasActiveEventsShouldReturnFalseIfUserHasNoActiveEvents()
        {
            var input = new LoginAndRegisterInputDto
            {
                UserName = "NewUser",
                Email = "newUser@example.com",
                Password = "NewUserPassword123"
            };

            UserDto newUser = await _userService.RegisterAsync(input);

            bool hasActiveEvents = await _userService.CheckIfUserHasActiveEvents(Guid.Parse(newUser.Id));
            Assert.That(hasActiveEvents, Is.False, $"User with ID {newUser.Id} should not have active events.");
        }

        [Test]
        public async Task DeleteUserAsyncShouldDeleteUserAndAssociatedParticipants()
        {
            Guid userId = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");
            await _userService.DeleteUserAsync(userId);
            bool userExists = await _userService.UserExistsByIdAsync(userId);
            Assert.That(userExists, Is.False, $"User with ID {userId} should be deleted.");
            // Check if all participants associated with the user are also deleted
            bool participantsExist = await _participantRepository.ExistsByExpressionAsync(p => p.UserId == userId);
            Assert.That(participantsExist, Is.False,
                "Participants associated with the deleted user should also be deleted.");
        }

        [Test]
        public async Task UpdateUserAsyncShouldUpdateOnlyEmailOfTheUserIfOnlyTheEmailIsProvided()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var updatedUser = new UpdateUserDto()
            {
                UserName = "",
                Email = "admin-updated@example.com",
                OldPassword = "",
                NewPassword = ""
            };

            UserDto? user = await _userService.UpdateUserAsync(userId, updatedUser);
            Assert.That(user, Is.Not.Null, "User should be updated.");
            Assert.That(user.UserName, Is.EqualTo("admin"), "User's username should remain unchanged.");
            Assert.That(user.Email, Is.EqualTo(updatedUser.Email), "Updated user's email does not match the input.");
        }

        [Test]
        public async Task UpdateUserAsyncShouldUpdateOnlyUsernameOfTheUserIfOnlyTheUsernameIsProvided()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updatedUser = new UpdateUserDto()
            {
                UserName = "Admin-Updated",
                Email = "",
                OldPassword = "",
                NewPassword = ""
            };
            UserDto? user = await _userService.UpdateUserAsync(userId, updatedUser);
            Assert.That(user, Is.Not.Null, "User should be updated.");
            Assert.That(user.UserName, Is.EqualTo(updatedUser.UserName),
                "Updated user's username does not match the input.");
            Assert.That(user.Email, Is.EqualTo("admin@example.com"), "User's email should remain unchanged.");
        }

        [Test]
        public async Task UpdateUserAsyncShouldUpdateOnlyPasswordOfTheUserIfOnlyThePasswordIsProvided()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            var updatedUser = new UpdateUserDto()
            {
                UserName = "",
                Email = "",
                OldPassword = "hashedpassword123",
                NewPassword = "newhashedpassword123"
            };
            UserDto? user = await _userService.UpdateUserAsync(userId, updatedUser);
            Assert.That(user, Is.Not.Null, "User should be updated.");
            Assert.That(user.UserName, Is.EqualTo("admin"), "User's username should remain unchanged.");
            Assert.That(user.Email, Is.EqualTo("admin@example.com"), "User's email should remain unchanged.");
        }

        [Test]
        public async Task UpdateUserAsyncShouldUpdateTheUser()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var updatedUser = new UpdateUserDto()
            {
                UserName = "Admin-Updated",
                Email = "admin-updated@example.com",
                OldPassword = "hashedpassword123",
                NewPassword = "updatedhashedpassword123"
            };

            UserDto? user = await _userService.UpdateUserAsync(userId, updatedUser);

            Assert.That(user, Is.Not.Null, "User should be updated.");
            Assert.That(user.UserName, Is.EqualTo(updatedUser.UserName),
                "Updated user's username does not match the input.");
            Assert.That(user.Email, Is.EqualTo(updatedUser.Email), "Updated user's email does not match the input.");
        }

        [Test]
        public async Task UpdateUserAsyncShouldThrowExceptionIfOldPasswordIsWrong()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var updatedUser = new UpdateUserDto()
            {
                UserName = "Admin-Updated",
                Email = "admin-updated@example.com",
                OldPassword = "hashedpassword1234222323",
                NewPassword = "updatedhashedpassword123"
            };

            var exception = Assert.ThrowsAsync<Exception>(async () =>
            {
                await _userService.UpdateUserAsync(userId, updatedUser);
            });

            Assert.That(exception, Is.Not.Null, "Exception should be thrown when old password is incorrect.");
            Assert.That(exception?.Message, Is.EqualTo("Old password is incorrect."),
                "Exception message does not match.");
        }

        [Test]
        public async Task UpdateUserAsyncShouldThrowExceptionIfOldAndNewPasswordsAreTheSame()
        {
            Guid userId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            var updatedUser = new UpdateUserDto()
            {
                UserName = "Admin-Updated",
                Email = "admin-updated@example.com",
                OldPassword = "hashedpassword123",
                NewPassword = "hashedpassword123"
            };

            var exception = Assert.ThrowsAsync<Exception>(async () =>
            {
                await _userService.UpdateUserAsync(userId, updatedUser);
            });

            Assert.That(exception, Is.Not.Null, "Exception should be thrown when old and new passwords are the same.");
            Assert.That(exception?.Message, Is.EqualTo("New password cannot be the same as the old password."),
                "Exception message does not match.");
        }

        [Test]
        public async Task GetUserParticipatingEventsAsyncShouldReturnTheEventsWhereUserIsParticipant()
        {
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            IEnumerable<EventDto> events = await _userService.GetUserParticipatingEventsAsync(user1Id);

            Assert.That(events, Is.Not.Null, "Events should not be null.");
            Assert.That(events.Count(), Is.EqualTo(3), "User should have participating events.");
            Assert.That(events.All(e => e.Participants.Any(p => p.Id == user1Id.ToString())),
                "All returned events should have the user as a participant.");
        }

        [Test]
        public async Task GetUserParticipatingEventsAsyncShouldReturnEmptyCollectionIfUserHasNoParticipatingEvents()
        {
            Guid userId = Guid.NewGuid(); // Random ID that does not exist
            IEnumerable<EventDto> events = await _userService.GetUserParticipatingEventsAsync(userId);
            Assert.That(events, Is.Not.Null, "Events should not be null.");
            Assert.That(events.Count(), Is.EqualTo(0), "User should not have participating events.");
        }

        [Test]
        public async Task GetUserEventsAsyncShouldReturnAllEventsCreatedByTheUserIFNoFilterIsApplied()
        {
            //You have to make some fixes in the method logic

            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");

            IEnumerable<EventDto> events = await _userService.GetUserEventsAsync(user1Id, null);

            Assert.That(events, Is.Not.Null, "Events should not be null.");
            Assert.That(events.Count(), Is.EqualTo(2), "User should have created events.");
            Assert.That(events.All(e => e.CreatorId == user1Id.ToString()),
                "All returned events should be created by the user.");

        }
    }
}
