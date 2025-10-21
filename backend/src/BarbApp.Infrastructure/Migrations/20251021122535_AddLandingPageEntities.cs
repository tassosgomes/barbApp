using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLandingPageEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "landing_page_configs",
                columns: table => new
                {
                    landing_page_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbershop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    about_text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    opening_hours = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    instagram_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    facebook_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    whatsapp_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_landing_page_configs", x => x.landing_page_config_id);
                    table.ForeignKey(
                        name: "FK_landing_page_configs_barbershops_barbershop_id",
                        column: x => x.barbershop_id,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "landing_page_services",
                columns: table => new
                {
                    landing_page_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    landing_page_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_landing_page_services", x => x.landing_page_service_id);
                    table.ForeignKey(
                        name: "FK_landing_page_services_barbershop_services_service_id",
                        column: x => x.service_id,
                        principalTable: "barbershop_services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_landing_page_services_landing_page_configs_landing_page_con~",
                        column: x => x.landing_page_config_id,
                        principalTable: "landing_page_configs",
                        principalColumn: "landing_page_config_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_landing_page_configs_is_published",
                table: "landing_page_configs",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "uq_landing_page_configs_barbershop",
                table: "landing_page_configs",
                column: "barbershop_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_landing_page_services_config_id",
                table: "landing_page_services",
                column: "landing_page_config_id");

            migrationBuilder.CreateIndex(
                name: "ix_landing_page_services_config_order",
                table: "landing_page_services",
                columns: new[] { "landing_page_config_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_landing_page_services_service_id",
                table: "landing_page_services",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "uq_landing_page_services_config_service",
                table: "landing_page_services",
                columns: new[] { "landing_page_config_id", "service_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "landing_page_services");

            migrationBuilder.DropTable(
                name: "landing_page_configs");
        }
    }
}
