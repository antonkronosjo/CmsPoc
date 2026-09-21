using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentRoots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ContentTypeKey = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentRoots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventContentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RootId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StopPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventContentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventContentVersions_ContentRoots_RootId",
                        column: x => x.RootId,
                        principalTable: "ContentRoots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsContentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RootId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StopPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RelatedContentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsContentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsContentVersions_ContentRoots_RootId",
                        column: x => x.RootId,
                        principalTable: "ContentRoots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestContentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RootId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StopPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RelatedContentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestContentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestContentVersions_ContentRoots_RootId",
                        column: x => x.RootId,
                        principalTable: "ContentRoots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventContentTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventContentTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventContentTranslations_EventContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "EventContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsContentTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    Heading = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsContentTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsContentTranslations_NewsContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "NewsContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestContentTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    Heading = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    TestProperty = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestContentTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestContentTranslations_TestContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "TestContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentRoots_ContentTypeKey",
                table: "ContentRoots",
                column: "ContentTypeKey");

            migrationBuilder.CreateIndex(
                name: "IX_ContentRoots_Name",
                table: "ContentRoots",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EventContentTranslations_VersionId_Language",
                table: "EventContentTranslations",
                columns: new[] { "VersionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventContentVersions_RootId_VersionNumber",
                table: "EventContentVersions",
                columns: new[] { "RootId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsContentTranslations_VersionId_Language",
                table: "NewsContentTranslations",
                columns: new[] { "VersionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsContentVersions_RootId_VersionNumber",
                table: "NewsContentVersions",
                columns: new[] { "RootId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestContentTranslations_VersionId_Language",
                table: "TestContentTranslations",
                columns: new[] { "VersionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestContentVersions_RootId_VersionNumber",
                table: "TestContentVersions",
                columns: new[] { "RootId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventContentTranslations");

            migrationBuilder.DropTable(
                name: "NewsContentTranslations");

            migrationBuilder.DropTable(
                name: "TestContentTranslations");

            migrationBuilder.DropTable(
                name: "EventContentVersions");

            migrationBuilder.DropTable(
                name: "NewsContentVersions");

            migrationBuilder.DropTable(
                name: "TestContentVersions");

            migrationBuilder.DropTable(
                name: "ContentRoots");
        }
    }
}
