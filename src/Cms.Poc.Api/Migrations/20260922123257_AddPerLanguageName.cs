using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerLanguageName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SpecialNewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SpecialNewsContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "NewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "NewsContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "EventContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "EventContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Backfill every existing row's Name from its ContentRoot before that
            // column is dropped below, so no existing content silently loses its name.
            migrationBuilder.Sql("UPDATE SpecialNewsContentVersions SET Name = (SELECT Name FROM ContentRoots WHERE ContentRoots.Id = SpecialNewsContentVersions.RootId);");
            migrationBuilder.Sql("UPDATE SpecialNewsContentTranslations SET Name = (SELECT Name FROM ContentRoots WHERE ContentRoots.Id = SpecialNewsContentTranslations.RootId);");
            migrationBuilder.Sql("UPDATE NewsContentVersions SET Name = (SELECT Name FROM ContentRoots WHERE ContentRoots.Id = NewsContentVersions.RootId);");
            migrationBuilder.Sql("UPDATE NewsContentTranslations SET Name = (SELECT Name FROM ContentRoots WHERE ContentRoots.Id = NewsContentTranslations.RootId);");
            migrationBuilder.Sql("UPDATE EventContentVersions SET Name = (SELECT Name FROM ContentRoots WHERE ContentRoots.Id = EventContentVersions.RootId);");
            migrationBuilder.Sql("UPDATE EventContentTranslations SET Name = (SELECT Name FROM ContentRoots WHERE ContentRoots.Id = EventContentTranslations.RootId);");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialNewsContentVersions_Name",
                table: "SpecialNewsContentVersions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialNewsContentTranslations_Name",
                table: "SpecialNewsContentTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NewsContentVersions_Name",
                table: "NewsContentVersions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NewsContentTranslations_Name",
                table: "NewsContentTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EventContentVersions_Name",
                table: "EventContentVersions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EventContentTranslations_Name",
                table: "EventContentTranslations",
                column: "Name");

            migrationBuilder.DropIndex(
                name: "IX_ContentRoots_Name",
                table: "ContentRoots");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ContentRoots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ContentRoots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Backfill from each root's master-language Version before the
            // per-language columns below are dropped, so the name isn't lost.
            migrationBuilder.Sql("UPDATE ContentRoots SET Name = COALESCE((SELECT v.Name FROM SpecialNewsContentVersions v WHERE v.RootId = ContentRoots.Id ORDER BY v.VersionNumber DESC LIMIT 1), Name) WHERE ContentTypeKey = 'SpecialNewsContent';");
            migrationBuilder.Sql("UPDATE ContentRoots SET Name = COALESCE((SELECT v.Name FROM NewsContentVersions v WHERE v.RootId = ContentRoots.Id ORDER BY v.VersionNumber DESC LIMIT 1), Name) WHERE ContentTypeKey = 'NewsContent';");
            migrationBuilder.Sql("UPDATE ContentRoots SET Name = COALESCE((SELECT v.Name FROM EventContentVersions v WHERE v.RootId = ContentRoots.Id ORDER BY v.VersionNumber DESC LIMIT 1), Name) WHERE ContentTypeKey = 'EventContent';");

            migrationBuilder.CreateIndex(
                name: "IX_ContentRoots_Name",
                table: "ContentRoots",
                column: "Name");

            migrationBuilder.DropIndex(
                name: "IX_SpecialNewsContentVersions_Name",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropIndex(
                name: "IX_SpecialNewsContentTranslations_Name",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_NewsContentVersions_Name",
                table: "NewsContentVersions");

            migrationBuilder.DropIndex(
                name: "IX_NewsContentTranslations_Name",
                table: "NewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_EventContentVersions_Name",
                table: "EventContentVersions");

            migrationBuilder.DropIndex(
                name: "IX_EventContentTranslations_Name",
                table: "EventContentTranslations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "NewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "EventContentVersions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "EventContentTranslations");
        }
    }
}
