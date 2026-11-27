using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareEMR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "patients",
                type: "boolean",
                nullable: false,
                // Existing patients must stay active (EF generated false, which would hide them all).
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "patients");
        }
    }
}
