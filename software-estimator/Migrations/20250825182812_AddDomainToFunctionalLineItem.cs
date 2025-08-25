using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace software_estimator.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainToFunctionalLineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "FunctionalLineItems",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domain",
                table: "FunctionalLineItems");
        }
    }
}
