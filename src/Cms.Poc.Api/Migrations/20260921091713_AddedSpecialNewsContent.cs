using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedSpecialNewsContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecialNewsContentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RootId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StopPublish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RelatedContent = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialNewsContentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialNewsContentVersions_ContentRoots_RootId",
                        column: x => x.RootId,
                        principalTable: "ContentRoots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialNewsContentTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    Heading = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    SpecialBody = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialNewsContentTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialNewsContentTranslations_SpecialNewsContentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "SpecialNewsContentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialNewsContentTranslations_VersionId_Language",
                table: "SpecialNewsContentTranslations",
                columns: new[] { "VersionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialNewsContentVersions_RootId_VersionNumber",
                table: "SpecialNewsContentVersions",
                columns: new[] { "RootId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialNewsContentTranslations");

            migrationBuilder.DropTable(
                name: "SpecialNewsContentVersions");
        }
    }
}
