﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_appointments_status",
                table: "appointments",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_appointments_status",
                table: "appointments");
        }
    }
}
