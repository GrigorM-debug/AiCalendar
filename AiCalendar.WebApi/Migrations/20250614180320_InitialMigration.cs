using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AiCalendar.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "The id of the user"),
                    UserName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, comment: "The username of the user"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "The email address of the user"),
                    PasswordHashed = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "The hashed user password")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "The id of the event"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "The title of the event"),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "The description of the event"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "The start time of the event"),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "The end time of the event"),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "The id of the user who created the event")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "The id of the user who participate in the event"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "The id of the event that user participate")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => new { x.UserId, x.EventId });
                    table.ForeignKey(
                        name: "FK_Participants_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "PasswordHashed", "UserName" },
                values: new object[,]
                {
                    { new Guid("11223344-5566-7788-99aa-bbccddeeff00"), "jessie@example.com", "hashedpassword789", "JessiePinkman" },
                    { new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"), "admin@example.com", "hashedpassword123", "admin" },
                    { new Guid("f0e9d8c7-b6a5-4321-fedc-ba9876543210"), "heisenberg@example.com", "hashedpassword456", "Heisenberg" }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "CreatorId", "Description", "EndTime", "StartTime", "Title" },
                values: new object[,]
                {
                    { new Guid("e1000000-0000-0000-0000-000000000001"), new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"), "Daily team synchronization meeting.", new DateTime(2025, 6, 16, 9, 30, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 16, 9, 0, 0, 0, DateTimeKind.Utc), "Team Stand-up Meeting" },
                    { new Guid("e1000000-0000-0000-0000-000000000002"), new Guid("f0e9d8c7-b6a5-4321-fedc-ba9876543210"), "Review progress on Project X with stakeholders.", new DateTime(2025, 6, 17, 15, 30, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 17, 14, 0, 0, 0, DateTimeKind.Utc), "Project X Review" },
                    { new Guid("e1000000-0000-0000-0000-000000000003"), new Guid("11223344-5566-7788-99aa-bbccddeeff00"), "Routine check-up.", new DateTime(2025, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 20, 8, 0, 0, 0, DateTimeKind.Utc), "Dentist Appointment" },
                    { new Guid("e1000000-0000-0000-0000-000000000004"), new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"), "Exploring the Vitosha mountains.", new DateTime(2025, 6, 21, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 21, 7, 0, 0, 0, DateTimeKind.Utc), "Weekend Hike" }
                });

            migrationBuilder.InsertData(
                table: "Participants",
                columns: new[] { "EventId", "UserId" },
                values: new object[,]
                {
                    { new Guid("e1000000-0000-0000-0000-000000000001"), new Guid("11223344-5566-7788-99aa-bbccddeeff00") },
                    { new Guid("e1000000-0000-0000-0000-000000000003"), new Guid("11223344-5566-7788-99aa-bbccddeeff00") },
                    { new Guid("e1000000-0000-0000-0000-000000000004"), new Guid("11223344-5566-7788-99aa-bbccddeeff00") },
                    { new Guid("e1000000-0000-0000-0000-000000000001"), new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef") },
                    { new Guid("e1000000-0000-0000-0000-000000000002"), new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef") },
                    { new Guid("e1000000-0000-0000-0000-000000000004"), new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef") },
                    { new Guid("e1000000-0000-0000-0000-000000000001"), new Guid("f0e9d8c7-b6a5-4321-fedc-ba9876543210") },
                    { new Guid("e1000000-0000-0000-0000-000000000002"), new Guid("f0e9d8c7-b6a5-4321-fedc-ba9876543210") },
                    { new Guid("e1000000-0000-0000-0000-000000000004"), new Guid("f0e9d8c7-b6a5-4321-fedc-ba9876543210") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatorId",
                table: "Events",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_EventId",
                table: "Participants",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
