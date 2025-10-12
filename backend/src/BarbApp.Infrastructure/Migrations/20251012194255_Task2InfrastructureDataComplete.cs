using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Task2InfrastructureDataComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "idx_barbershops_code",
                table: "barbershops",
                newName: "IX_barbershops_code");

            migrationBuilder.AddColumn<Guid>(
                name: "BarbeariaId1",
                table: "customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "address_id",
                table: "barbershops",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "barbershops",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "document",
                table: "barbershops",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "document_type",
                table: "barbershops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "barbershops",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "owner_name",
                table: "barbershops",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "barbershops",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "barbershops",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BarbeariaId1",
                table: "barbers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BarbeariaId1",
                table: "admin_barbearia_users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    complement = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    neighborhood = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.address_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customers_BarbeariaId1",
                table: "customers",
                column: "BarbeariaId1");

            migrationBuilder.CreateIndex(
                name: "idx_barbershops_is_active",
                table: "barbershops",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_barbershops_name",
                table: "barbershops",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_barbershops_address_id",
                table: "barbershops",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_barbershops_document",
                table: "barbershops",
                column: "document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_barbers_BarbeariaId1",
                table: "barbers",
                column: "BarbeariaId1");

            migrationBuilder.CreateIndex(
                name: "IX_admin_barbearia_users_BarbeariaId1",
                table: "admin_barbearia_users",
                column: "BarbeariaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_admin_barbearia_users_barbershops_BarbeariaId1",
                table: "admin_barbearia_users",
                column: "BarbeariaId1",
                principalTable: "barbershops",
                principalColumn: "barbershop_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_barbers_barbershops_BarbeariaId1",
                table: "barbers",
                column: "BarbeariaId1",
                principalTable: "barbershops",
                principalColumn: "barbershop_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_barbershops_addresses_address_id",
                table: "barbershops",
                column: "address_id",
                principalTable: "addresses",
                principalColumn: "address_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_customers_barbershops_BarbeariaId1",
                table: "customers",
                column: "BarbeariaId1",
                principalTable: "barbershops",
                principalColumn: "barbershop_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_admin_barbearia_users_barbershops_BarbeariaId1",
                table: "admin_barbearia_users");

            migrationBuilder.DropForeignKey(
                name: "FK_barbers_barbershops_BarbeariaId1",
                table: "barbers");

            migrationBuilder.DropForeignKey(
                name: "FK_barbershops_addresses_address_id",
                table: "barbershops");

            migrationBuilder.DropForeignKey(
                name: "FK_customers_barbershops_BarbeariaId1",
                table: "customers");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropIndex(
                name: "IX_customers_BarbeariaId1",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "idx_barbershops_is_active",
                table: "barbershops");

            migrationBuilder.DropIndex(
                name: "idx_barbershops_name",
                table: "barbershops");

            migrationBuilder.DropIndex(
                name: "IX_barbershops_address_id",
                table: "barbershops");

            migrationBuilder.DropIndex(
                name: "IX_barbershops_document",
                table: "barbershops");

            migrationBuilder.DropIndex(
                name: "IX_barbers_BarbeariaId1",
                table: "barbers");

            migrationBuilder.DropIndex(
                name: "IX_admin_barbearia_users_BarbeariaId1",
                table: "admin_barbearia_users");

            migrationBuilder.DropColumn(
                name: "BarbeariaId1",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "address_id",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "document",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "document_type",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "email",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "owner_name",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "barbershops");

            migrationBuilder.DropColumn(
                name: "BarbeariaId1",
                table: "barbers");

            migrationBuilder.DropColumn(
                name: "BarbeariaId1",
                table: "admin_barbearia_users");

            migrationBuilder.RenameIndex(
                name: "IX_barbershops_code",
                table: "barbershops",
                newName: "idx_barbershops_code");
        }
    }
}
