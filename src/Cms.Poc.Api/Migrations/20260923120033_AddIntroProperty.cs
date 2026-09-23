using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIntroProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Intro",
                table: "SpecialNewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intro",
                table: "SpecialNewsContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intro",
                table: "NewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intro",
                table: "NewsContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intro",
                table: "EventContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intro",
                table: "EventContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Intro",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Intro",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "Intro",
                table: "NewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Intro",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "Intro",
                table: "EventContentVersions");

            migrationBuilder.DropColumn(
                name: "Intro",
                table: "EventContentTranslations");
        }
    }
}
