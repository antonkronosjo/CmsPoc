using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class PerLanguageVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventContentTranslations_EventContentVersions_VersionId",
                table: "EventContentTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsContentTranslations_NewsContentVersions_VersionId",
                table: "NewsContentTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecialNewsContentTranslations_SpecialNewsContentVersions_VersionId",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_SpecialNewsContentTranslations_VersionId_Language",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_NewsContentTranslations_VersionId_Language",
                table: "NewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_EventContentTranslations_VersionId_Language",
                table: "EventContentTranslations");

            migrationBuilder.RenameColumn(
                name: "VersionId",
                table: "SpecialNewsContentTranslations",
                newName: "VersionNumber");

            migrationBuilder.RenameColumn(
                name: "VersionId",
                table: "NewsContentTranslations",
                newName: "VersionNumber");

            migrationBuilder.RenameColumn(
                name: "VersionId",
                table: "EventContentTranslations",
                newName: "VersionNumber");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "SpecialNewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Heading",
                table: "SpecialNewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialBody",
                table: "SpecialNewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "SpecialNewsContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SpecialNewsContentTranslations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "SpecialNewsContentTranslations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RootId",
                table: "SpecialNewsContentTranslations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartPublish",
                table: "SpecialNewsContentTranslations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StopPublish",
                table: "SpecialNewsContentTranslations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "NewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Heading",
                table: "NewsContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "NewsContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "NewsContentTranslations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "NewsContentTranslations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RootId",
                table: "NewsContentTranslations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartPublish",
                table: "NewsContentTranslations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StopPublish",
                table: "NewsContentTranslations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EventContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "EventContentVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "EventContentTranslations",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EventContentTranslations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "EventContentTranslations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RootId",
                table: "EventContentTranslations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartPublish",
                table: "EventContentTranslations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StopPublish",
                table: "EventContentTranslations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialNewsContentTranslations_RootId_Language_VersionNumber",
                table: "SpecialNewsContentTranslations",
                columns: new[] { "RootId", "Language", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsContentTranslations_RootId_Language_VersionNumber",
                table: "NewsContentTranslations",
                columns: new[] { "RootId", "Language", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventContentTranslations_RootId_Language_VersionNumber",
                table: "EventContentTranslations",
                columns: new[] { "RootId", "Language", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventContentTranslations_ContentRoots_RootId",
                table: "EventContentTranslations",
                column: "RootId",
                principalTable: "ContentRoots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsContentTranslations_ContentRoots_RootId",
                table: "NewsContentTranslations",
                column: "RootId",
                principalTable: "ContentRoots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialNewsContentTranslations_ContentRoots_RootId",
                table: "SpecialNewsContentTranslations",
                column: "RootId",
                principalTable: "ContentRoots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventContentTranslations_ContentRoots_RootId",
                table: "EventContentTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsContentTranslations_ContentRoots_RootId",
                table: "NewsContentTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecialNewsContentTranslations_ContentRoots_RootId",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_SpecialNewsContentTranslations_RootId_Language_VersionNumber",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_NewsContentTranslations_RootId_Language_VersionNumber",
                table: "NewsContentTranslations");

            migrationBuilder.DropIndex(
                name: "IX_EventContentTranslations_RootId_Language_VersionNumber",
                table: "EventContentTranslations");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Heading",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropColumn(
                name: "SpecialBody",
                table: "SpecialNewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "RootId",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "StartPublish",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "StopPublish",
                table: "SpecialNewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "NewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Heading",
                table: "NewsContentVersions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "RootId",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "StartPublish",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "StopPublish",
                table: "NewsContentTranslations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EventContentVersions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "EventContentVersions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "EventContentTranslations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EventContentTranslations");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "EventContentTranslations");

            migrationBuilder.DropColumn(
                name: "RootId",
                table: "EventContentTranslations");

            migrationBuilder.DropColumn(
                name: "StartPublish",
                table: "EventContentTranslations");

            migrationBuilder.DropColumn(
                name: "StopPublish",
                table: "EventContentTranslations");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "SpecialNewsContentTranslations",
                newName: "VersionId");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "NewsContentTranslations",
                newName: "VersionId");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "EventContentTranslations",
                newName: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialNewsContentTranslations_VersionId_Language",
                table: "SpecialNewsContentTranslations",
                columns: new[] { "VersionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsContentTranslations_VersionId_Language",
                table: "NewsContentTranslations",
                columns: new[] { "VersionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventContentTranslations_VersionId_Language",
                table: "EventContentTranslations",
                columns: new[] { "VersionId", "Language" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventContentTranslations_EventContentVersions_VersionId",
                table: "EventContentTranslations",
                column: "VersionId",
                principalTable: "EventContentVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsContentTranslations_NewsContentVersions_VersionId",
                table: "NewsContentTranslations",
                column: "VersionId",
                principalTable: "NewsContentVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialNewsContentTranslations_SpecialNewsContentVersions_VersionId",
                table: "SpecialNewsContentTranslations",
                column: "VersionId",
                principalTable: "SpecialNewsContentVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
