using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedContentReferenceStruct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedContentId",
                table: "NewsContentVersions");

            migrationBuilder.AddColumn<string>(
                name: "RelatedContent",
                table: "NewsContentVersions",
                type: "TEXT",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedContent",
                table: "NewsContentVersions");

            migrationBuilder.AddColumn<int>(
                name: "RelatedContentId",
                table: "NewsContentVersions",
                type: "INTEGER",
                nullable: true);
        }
    }
}
