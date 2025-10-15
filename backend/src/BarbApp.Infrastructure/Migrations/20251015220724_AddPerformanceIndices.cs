using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_barbers_barbearia_is_active",
                table: "barbers",
                columns: new[] { "barbearia_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_appointments_barber_start_time",
                table: "appointments",
                columns: new[] { "barber_id", "start_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_barbers_barbearia_is_active",
                table: "barbers");

            migrationBuilder.DropIndex(
                name: "ix_appointments_barber_start_time",
                table: "appointments");
        }
    }
}
