using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace software_estimator.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimateEnhancementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JiraIdeaUrl",
                table: "Estimates",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraInitiativeUrl",
                table: "Estimates",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreparedBy",
                table: "Estimates",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProblemStatement",
                table: "Estimates",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JiraIdeaUrl",
                table: "Estimates");

            migrationBuilder.DropColumn(
                name: "JiraInitiativeUrl",
                table: "Estimates");

            migrationBuilder.DropColumn(
                name: "PreparedBy",
                table: "Estimates");

            migrationBuilder.DropColumn(
                name: "ProblemStatement",
                table: "Estimates");
        }
    }
}
