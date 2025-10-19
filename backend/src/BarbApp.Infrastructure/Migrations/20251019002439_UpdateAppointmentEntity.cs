using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "service_name",
                table: "appointments");

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmed_at",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_appointments_service_id",
                table: "appointments",
                column: "service_id");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_barbershop_services_service_id",
                table: "appointments",
                column: "service_id",
                principalTable: "barbershop_services",
                principalColumn: "service_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_barbershop_services_service_id",
                table: "appointments");

            migrationBuilder.DropIndex(
                name: "IX_appointments_service_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "appointments");

            migrationBuilder.AddColumn<string>(
                name: "service_name",
                table: "appointments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
