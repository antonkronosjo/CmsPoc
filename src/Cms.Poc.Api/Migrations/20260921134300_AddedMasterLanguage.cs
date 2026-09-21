using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Poc.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedMasterLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MasterLanguage",
                table: "ContentRoots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Everything created before master languages existed was implicitly
            // English (the old global default).
            migrationBuilder.Sql("UPDATE ContentRoots SET MasterLanguage = 'en' WHERE MasterLanguage = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasterLanguage",
                table: "ContentRoots");
        }
    }
}
