using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBarberEmailAuthAndBarbershopServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_barbers_telefone_barbearia_id",
                table: "barbers");

            migrationBuilder.RenameColumn(
                name: "telefone",
                table: "barbers",
                newName: "phone");

            migrationBuilder.RenameIndex(
                name: "ix_barbers_telefone",
                table: "barbers",
                newName: "ix_barbers_phone");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "barbers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "barbers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "barbers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "service_ids",
                table: "barbers",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "barbershop_services",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbearia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barbershop_services", x => x.service_id);
                    table.ForeignKey(
                        name: "FK_barbershop_services_barbershops_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_barbers_email",
                table: "barbers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "uq_barbers_barbearia_email",
                table: "barbers",
                columns: new[] { "barbearia_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_barbershop_services_barbearia_id",
                table: "barbershop_services",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_barbershop_services_barbearia_name",
                table: "barbershop_services",
                columns: new[] { "barbearia_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_barbershop_services_is_active",
                table: "barbershop_services",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "barbershop_services");

            migrationBuilder.DropIndex(
                name: "ix_barbers_email",
                table: "barbers");

            migrationBuilder.DropIndex(
                name: "uq_barbers_barbearia_email",
                table: "barbers");

            migrationBuilder.DropColumn(
                name: "email",
                table: "barbers");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "barbers");

            migrationBuilder.DropColumn(
                name: "service_ids",
                table: "barbers");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "barbers",
                newName: "telefone");

            migrationBuilder.RenameIndex(
                name: "ix_barbers_phone",
                table: "barbers",
                newName: "ix_barbers_telefone");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "barbers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "ix_barbers_telefone_barbearia_id",
                table: "barbers",
                columns: new[] { "telefone", "barbearia_id" },
                unique: true);
        }
    }
}
