using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace software_estimator.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamIdToEstimate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamId",
                table: "Estimates",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Estimates");
        }
    }
}
