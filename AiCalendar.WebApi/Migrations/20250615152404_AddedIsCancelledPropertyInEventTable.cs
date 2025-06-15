using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCalendar.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsCancelledPropertyInEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "If event is cancelled or no");

            //migrationBuilder.UpdateData(
            //    table: "Events",
            //    keyColumn: "Id",
            //    keyValue: new Guid("e1000000-0000-0000-0000-000000000001"),
            //    columns: new string[0],
            //    values: new object[0]);

            //migrationBuilder.UpdateData(
            //    table: "Events",
            //    keyColumn: "Id",
            //    keyValue: new Guid("e1000000-0000-0000-0000-000000000002"),
            //    columns: new string[0],
            //    values: new object[0]);

            //migrationBuilder.UpdateData(
            //    table: "Events",
            //    keyColumn: "Id",
            //    keyValue: new Guid("e1000000-0000-0000-0000-000000000003"),
            //    columns: new string[0],
            //    values: new object[0]);

            //migrationBuilder.UpdateData(
            //    table: "Events",
            //    keyColumn: "Id",
            //    keyValue: new Guid("e1000000-0000-0000-0000-000000000004"),
            //    columns: new string[0],
            //    values: new object[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Events");
        }
    }
}
