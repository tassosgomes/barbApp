using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientControllers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbearia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telefone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.cliente_id);
                    table.ForeignKey(
                        name: "FK_clientes_barbershops_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agendamentos",
                columns: table => new
                {
                    agendamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbearia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barbeiro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duracao_total = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agendamentos", x => x.agendamento_id);
                    table.ForeignKey(
                        name: "FK_agendamentos_barbers_barbeiro_id",
                        column: x => x.barbeiro_id,
                        principalTable: "barbers",
                        principalColumn: "barber_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agendamentos_barbershops_barbearia_id",
                        column: x => x.barbearia_id,
                        principalTable: "barbershops",
                        principalColumn: "barbershop_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agendamentos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agendamento_servicos",
                columns: table => new
                {
                    agendamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agendamento_servicos", x => new { x.agendamento_id, x.servico_id });
                    table.ForeignKey(
                        name: "FK_agendamento_servicos_agendamentos_agendamento_id",
                        column: x => x.agendamento_id,
                        principalTable: "agendamentos",
                        principalColumn: "agendamento_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agendamento_servicos_barbershop_services_servico_id",
                        column: x => x.servico_id,
                        principalTable: "barbershop_services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_agendamento_servicos_agendamento",
                table: "agendamento_servicos",
                column: "agendamento_id");

            migrationBuilder.CreateIndex(
                name: "idx_agendamento_servicos_servico",
                table: "agendamento_servicos",
                column: "servico_id");

            migrationBuilder.CreateIndex(
                name: "idx_agendamentos_barbearia",
                table: "agendamentos",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "idx_agendamentos_barbeiro_data",
                table: "agendamentos",
                columns: new[] { "barbeiro_id", "data_hora" });

            migrationBuilder.CreateIndex(
                name: "idx_agendamentos_cliente_status",
                table: "agendamentos",
                columns: new[] { "cliente_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_agendamentos_data_hora",
                table: "agendamentos",
                column: "data_hora");

            migrationBuilder.CreateIndex(
                name: "idx_clientes_barbearia",
                table: "clientes",
                column: "barbearia_id");

            migrationBuilder.CreateIndex(
                name: "idx_clientes_telefone_barbearia",
                table: "clientes",
                columns: new[] { "telefone", "barbearia_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agendamento_servicos");

            migrationBuilder.DropTable(
                name: "agendamentos");

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
