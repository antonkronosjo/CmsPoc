using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class Removed_ContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestContentTranslations");

            migrationBuilder.DropTable(
                name: "TestContentVersions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestContentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RootId = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RelatedContentId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StopPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "TestContentTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    Heading = table.Column<string>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false)
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
    }
}
