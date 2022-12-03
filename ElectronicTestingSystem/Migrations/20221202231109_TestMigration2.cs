using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicTestingSystem.Migrations
{
    /// <inheritdoc />
    public partial class TestMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "RequestedExams");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RequestedExams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RequestedExams");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "RequestedExams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
