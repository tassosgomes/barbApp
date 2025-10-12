using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesForTask14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_admin_barbearia_users_barbershops_BarbershopId",
                table: "admin_barbearia_users");

            migrationBuilder.DropForeignKey(
                name: "FK_barbers_barbershops_BarbershopId",
                table: "barbers");

            migrationBuilder.DropForeignKey(
                name: "FK_customers_barbershops_BarbershopId",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "idx_customers_barbearia_telefone",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_BarbershopId",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "idx_barbers_barbearia_telefone",
                table: "barbers");

            migrationBuilder.DropIndex(
                name: "IX_barbers_BarbershopId",
                table: "barbers");

            migrationBuilder.DropIndex(
                name: "idx_admin_barbearia_users_barbearia_email",
                table: "admin_barbearia_users");

            migrationBuilder.DropIndex(
                name: "IX_admin_barbearia_users_BarbershopId",
                table: "admin_barbearia_users");

            migrationBuilder.DropColumn(
                name: "BarbershopId",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "BarbershopId",
                table: "barbers");

            migrationBuilder.DropColumn(
                name: "BarbershopId",
                table: "admin_barbearia_users");

            migrationBuilder.RenameIndex(
                name: "idx_customers_telefone",
                table: "customers",
                newName: "ix_customers_telefone");

            migrationBuilder.RenameIndex(
                name: "idx_barbers_telefone",
                table: "barbers",
                newName: "ix_barbers_telefone");

            migrationBuilder.RenameIndex(
                name: "idx_admin_barbearia_users_barbearia_id",
                table: "admin_barbearia_users",
                newName: "ix_admin_barbearia_users_barbearia_id");

            migrationBuilder.AlterColumn<string>(
                name: "telefone",
                table: "customers",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "telefone",
                table: "barbers",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "admin_barbearia_users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "ix_customers_barbearia_id",
                table: "customers",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_telefone_barbearia_id",
                table: "customers",
                columns: new[] { "telefone", "barbearia_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_barbers_barbearia_id",
                table: "barbers",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "ix_barbers_telefone_barbearia_id",
                table: "barbers",
                columns: new[] { "telefone", "barbearia_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_admin_barbearia_users_email",
                table: "admin_barbearia_users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_admin_barbearia_users_email_barbearia_id",
                table: "admin_barbearia_users",
                columns: new[] { "email", "barbearia_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customers_barbearia_id",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "ix_customers_telefone_barbearia_id",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "ix_barbers_barbearia_id",
                table: "barbers");

            migrationBuilder.DropIndex(
                name: "ix_barbers_telefone_barbearia_id",
                table: "barbers");

            migrationBuilder.DropIndex(
                name: "ix_admin_barbearia_users_email",
                table: "admin_barbearia_users");

            migrationBuilder.DropIndex(
                name: "ix_admin_barbearia_users_email_barbearia_id",
                table: "admin_barbearia_users");

            migrationBuilder.RenameIndex(
                name: "ix_customers_telefone",
                table: "customers",
                newName: "idx_customers_telefone");

            migrationBuilder.RenameIndex(
                name: "ix_barbers_telefone",
                table: "barbers",
                newName: "idx_barbers_telefone");

            migrationBuilder.RenameIndex(
                name: "ix_admin_barbearia_users_barbearia_id",
                table: "admin_barbearia_users",
                newName: "idx_admin_barbearia_users_barbearia_id");

            migrationBuilder.AlterColumn<string>(
                name: "telefone",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);

            migrationBuilder.AddColumn<Guid>(
                name: "BarbershopId",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "telefone",
                table: "barbers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);

            migrationBuilder.AddColumn<Guid>(
                name: "BarbershopId",
                table: "barbers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "admin_barbearia_users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "BarbershopId",
                table: "admin_barbearia_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_customers_barbearia_telefone",
                table: "customers",
                columns: new[] { "barbearia_id", "telefone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_BarbershopId",
                table: "customers",
                column: "BarbershopId");

            migrationBuilder.CreateIndex(
                name: "idx_barbers_barbearia_telefone",
                table: "barbers",
                columns: new[] { "barbearia_id", "telefone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_barbers_BarbershopId",
                table: "barbers",
                column: "BarbershopId");

            migrationBuilder.CreateIndex(
                name: "idx_admin_barbearia_users_barbearia_email",
                table: "admin_barbearia_users",
                columns: new[] { "barbearia_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_barbearia_users_BarbershopId",
                table: "admin_barbearia_users",
                column: "BarbershopId");

            migrationBuilder.AddForeignKey(
                name: "FK_admin_barbearia_users_barbershops_BarbershopId",
                table: "admin_barbearia_users",
                column: "BarbershopId",
                principalTable: "barbershops",
                principalColumn: "barbershop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_barbers_barbershops_BarbershopId",
                table: "barbers",
                column: "BarbershopId",
                principalTable: "barbershops",
                principalColumn: "barbershop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_customers_barbershops_BarbershopId",
                table: "customers",
                column: "BarbershopId",
                principalTable: "barbershops",
                principalColumn: "barbershop_id");
        }
    }
}
