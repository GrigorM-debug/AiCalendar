using AiCalendar.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!; 

        public DbSet<Participant> Participants { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure your entities here

            modelBuilder.Entity<Event>()
                .Property(e => e.IsCancelled)
                .HasDefaultValue(false);

            //Seed some data for users and events

            // --Seed Users
            Guid user1Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
            Guid user2Id = Guid.Parse("F0E9D8C7-B6A5-4321-FEDC-BA9876543210");
            Guid user3Id = Guid.Parse("11223344-5566-7788-99AA-BBCCDDEEFF00");

            // I forgot to hash passwords
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = user1Id,
                    UserName = "admin",
                    Email = "admin@example.com",
                    PasswordHashed = "hashedpassword123"
                }, 
                new User
                {
                    Id = user2Id,
                    UserName = "Heisenberg",
                    Email = "heisenberg@example.com",
                    PasswordHashed = "hashedpassword456"
                },
                new User
                {
                    Id = user3Id,
                    UserName = "JessiePinkman",
                    Email = "jessie@example.com",
                    PasswordHashed = "hashedpassword789"
                }
                );

            // --Seed Events
            Guid event1Id = Guid.Parse("E1000000-0000-0000-0000-000000000001");
            Guid event2Id = Guid.Parse("E1000000-0000-0000-0000-000000000002");
            Guid event3Id = Guid.Parse("E1000000-0000-0000-0000-000000000003");
            Guid event4Id = Guid.Parse("E1000000-0000-0000-0000-000000000004");

            modelBuilder.Entity<Event>().HasData(
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
                }
            );

            // --Seed Participants
            modelBuilder.Entity<Participant>().HasData(
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
            );
        }
    }
}
