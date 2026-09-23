using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEventEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "EventContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Existing events have no end date - default them to their start date rather than year 1.
            migrationBuilder.Sql("UPDATE EventContentVersions SET EndDate = StartDate;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "EventContentVersions");
        }
    }
}
