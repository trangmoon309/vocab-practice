using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocaPlay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishDefinitionToWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishDefinition",
                table: "Words",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishDefinition",
                table: "Words");
        }
    }
}
