using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAddressesAndBarbershopRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create addresses table
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
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.address_id);
                });

            // Add address_id column to barbershops
            migrationBuilder.AddColumn<Guid>(
                name: "address_id",
                table: "barbershops",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            // Add other barbershop columns
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

            // Create indexes
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

            // Create foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_barbershops_addresses_address_id",
                table: "barbershops",
                column: "address_id",
                principalTable: "addresses",
                principalColumn: "address_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_barbershops_addresses_address_id",
                table: "barbershops");

            migrationBuilder.DropTable(
                name: "addresses");

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
        }
    }
}
