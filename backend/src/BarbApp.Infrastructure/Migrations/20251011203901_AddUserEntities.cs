using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_central_users",
                columns: table => new
                {
                    admin_central_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_central_users", x => x.admin_central_user_id);
                });

            migrationBuilder.CreateTable(
                name: "barbershops",
                columns: table => new
                {
                    barbershop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barbershops", x => x.barbershop_id);
                });

            migrationBuilder.CreateTable(
                name: "admin_barbearia_users",
                columns: table => new
                {
                    admin_barbearia_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbearia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BarbershopId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_barbearia_users", x => x.admin_barbearia_user_id);
                    table.ForeignKey(
                        name: "FK_admin_barbearia_users_barbershops_BarbershopId",
                        column: x => x.BarbershopId,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id");
                    table.ForeignKey(
                        name: "FK_admin_barbearia_users_barbershops_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "barbers",
                columns: table => new
                {
                    barber_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbearia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BarbershopId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barbers", x => x.barber_id);
                    table.ForeignKey(
                        name: "FK_barbers_barbershops_BarbershopId",
                        column: x => x.BarbershopId,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id");
                    table.ForeignKey(
                        name: "FK_barbers_barbershops_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbearia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BarbershopId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.customer_id);
                    table.ForeignKey(
                        name: "FK_customers_barbershops_BarbershopId",
                        column: x => x.BarbershopId,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id");
                    table.ForeignKey(
                        name: "FK_customers_barbershops_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_admin_barbearia_users_barbearia_email",
                table: "admin_barbearia_users",
                columns: new[] { "barbearia_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_admin_barbearia_users_barbearia_id",
                table: "admin_barbearia_users",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "IX_admin_barbearia_users_BarbershopId",
                table: "admin_barbearia_users",
                column: "BarbershopId");

            migrationBuilder.CreateIndex(
                name: "idx_admin_central_users_email",
                table: "admin_central_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_barbers_barbearia_telefone",
                table: "barbers",
                columns: new[] { "barbearia_id", "telefone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_barbers_telefone",
                table: "barbers",
                column: "telefone");

            migrationBuilder.CreateIndex(
                name: "IX_barbers_BarbershopId",
                table: "barbers",
                column: "BarbershopId");

            migrationBuilder.CreateIndex(
                name: "idx_barbershops_code",
                table: "barbershops",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_customers_barbearia_telefone",
                table: "customers",
                columns: new[] { "barbearia_id", "telefone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_customers_telefone",
                table: "customers",
                column: "telefone");

            migrationBuilder.CreateIndex(
                name: "IX_customers_BarbershopId",
                table: "customers",
                column: "BarbershopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_barbearia_users");

            migrationBuilder.DropTable(
                name: "admin_central_users");

            migrationBuilder.DropTable(
                name: "barbers");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "barbershops");
        }
    }
}
