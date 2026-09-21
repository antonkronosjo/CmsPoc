using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SpecialNewsContentVersions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "SpecialNewsContentVersions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "NewsContentVersions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "NewsContentVersions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EventContentVersions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "EventContentVersions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "NewsContentVersions");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "NewsContentVersions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EventContentVersions");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "EventContentVersions");
        }
    }
}
