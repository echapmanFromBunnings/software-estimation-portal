using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace software_estimator.Migrations
{
    /// <inheritdoc />
    public partial class AddOutcomeToFunctionalLineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "FunctionalLineItems",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "FunctionalLineItems");
        }
    }
}
