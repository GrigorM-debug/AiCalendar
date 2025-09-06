using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiCalendar.WebApi.Data;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.Tests
{
    public abstract class InMemoryDbTestBase
    {
        protected ApplicationDbContext _context;
        protected IPasswordHasher _passwordHasher;

        protected InMemoryDbTestBase()
        {
            _passwordHasher = new PasswordHasher();
        }

        protected async Task Init()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"AiCalendarDb{Guid.NewGuid().ToString()}")
                .Options;

            _context = new ApplicationDbContext(options);

            await _context.Database.EnsureDeletedAsync();
            await PopulateData();
            await _context.SaveChangesAsync();
        }

        protected async Task Dispose()
        {   
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        private async Task PopulateData()
        {
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            Guid user2Id = Guid.Parse("F0E9D8C7-B6A5-4321-FEDC-BA9876543210");
            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");

            IEnumerable<User> users = new List<User>
            {
                new User
                {
                    Id = user1Id,
                    UserName = "admin",
                    Email = "admin@example.com",
                    PasswordHashed = _passwordHasher.HashPassword("hashedpassword123")
                },
                new User
                {
                    Id = user2Id,
                    UserName = "Heisenberg",
                    Email = "heisenberg@example.com",
                    PasswordHashed = _passwordHasher.HashPassword("hashedpassword456")
                },
                new User
                {
                    Id = user3Id,
                    UserName = "JessiePinkman",
                    Email = "jessie@example.com",
                    PasswordHashed = _passwordHasher.HashPassword("hashedpassword789")
                }
            };

            await _context.Users.AddRangeAsync(users);

            Guid event1Id = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            Guid event2Id = Guid.Parse("E1000000-0000-0000-0000-000000000002");
            Guid event3Id = Guid.Parse("E1000000-0000-0000-0000-000000000003");
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");
            Guid event5Id = Guid.Parse("E1000000-0000-0000-0000-000000000005");
            Guid event6Id = Guid.Parse("E1000000-0000-0000-0000-000000000006");

            IEnumerable<Event> events = new List<Event>
            {
                new Event
                {
                    Id = event1Id,
                    Title = "Team Stand-up Meeting",
                    Description = "Daily team synchronization meeting.",
                    // Use fixed DateTime values with Kind.Utc
                    StartTime = new DateTime(2025, 6, 16, 9, 0, 0, DateTimeKind.Utc), // Monday, June 16, 9:00 AM UTC
                    EndTime = new DateTime(2025, 6, 16, 9, 30, 0, DateTimeKind.Utc), // Monday, June 16, 9:30 AM UTC
                    CreatorId = user1Id, // Created by admin
                },
                new Event
                {
                    Id = event2Id,
                    Title = "Project X Review",
                    Description = "Review progress on Project X with stakeholders.",
                    StartTime = new DateTime(2025, 6, 17, 14, 0, 0, DateTimeKind.Utc), // Tuesday, June 17, 2:00 PM UTC
                    EndTime = new DateTime(2025, 6, 17, 15, 30, 0, DateTimeKind.Utc), // Tuesday, June 17, 3:30 PM UTC
                    CreatorId = user2Id, // Created by Heisenberg
                },
                new Event
                {
                    Id = event3Id,
                    Title = "Dentist Appointment",
                    Description = "Routine check-up.",
                    StartTime = new DateTime(2025, 6, 20, 8, 0, 0, DateTimeKind.Utc), // Friday, June 20, 8:00 AM UTC
                    EndTime = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc), // Friday, June 20, 9:00 AM UTC
                    CreatorId = user3Id, // Created by JessiePinkman
                },
                new Event
                {
                    Id = event4Id,
                    Title = "Weekend Hike",
                    Description = "Exploring the Vitosha mountains.",
                    StartTime = new DateTime(2025, 6, 21, 7, 0, 0, DateTimeKind.Utc), // Saturday, June 21, 7:00 AM UTC
                    EndTime = new DateTime(2025, 6, 21, 15, 0, 0, DateTimeKind.Utc), // Saturday, June 21, 3:00 PM UTC
                    CreatorId = user1Id, // Created by admin
                },
                new Event()
                {
                    Id = event5Id,
                    Title = "Lunch with Sarah",
                    Description = "Catching up with Sarah at the new cafe.",
                    StartTime = new DateTime(2025, 6, 18, 12, 30, 0, DateTimeKind.Utc), // Wednesday, June 18, 12:30 PM UTC
                    EndTime = new DateTime(2025, 6, 18, 13, 30, 0, DateTimeKind.Utc), // Wednesday, June 18, 1:30 PM UTC
                    CreatorId = user2Id, // Created by Heisenberg
                    IsCancelled = true
                },
                new Event()
                {
                    Id = event6Id,
                    Title = "Code Review Session",
                    Description = "Reviewing code for the new feature implementation.",
                    StartTime = new DateTime(2025, 6, 19, 10, 0, 0, DateTimeKind.Utc), // Thursday, June 19, 10:00 AM UTC
                    EndTime = new DateTime(2025, 6, 19, 11, 30, 0, DateTimeKind.Utc), // Thursday, June 19, 11:30 AM UTC
                    CreatorId = user1Id, // Created by admin
                    IsCancelled = true
                }
            };

            await _context.Events.AddRangeAsync(events);

            IEnumerable<Participant> participants = new List<Participant>
            {
                // user1 (admin) participates in event1 (created by user1)
                new Participant { UserId = user1Id, EventId = event1Id },
                // user2 (Heisenberg) participates in event1
                new Participant { UserId = user2Id, EventId = event1Id },
                // user3 (JessiePinkman) participates in event1
                new Participant { UserId = user3Id, EventId = event1Id },

                // user1 (admin) participates in event2 (created by user2)
                new Participant { UserId = user1Id, EventId = event2Id },
                // user2 (Heisenberg) participates in event2
                new Participant { UserId = user2Id, EventId = event2Id },

                // user3 (JessiePinkman) participates in event3 (created by user3)
                new Participant { UserId = user3Id, EventId = event3Id },

                // user1 (admin) participates in event4 (created by user1)
                new Participant { UserId = user1Id, EventId = event4Id },
                // user2 (Heisenberg) participates in event4
                new Participant { UserId = user2Id, EventId = event4Id },
                // user3 (JessiePinkman) participates in event4
                new Participant { UserId = user3Id, EventId = event4Id }
            };

            await _context.Participants.AddRangeAsync(participants);

            await _context.SaveChangesAsync();
        }
    }
}
