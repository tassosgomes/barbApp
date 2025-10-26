using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAgendamentosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE agendamentos (
                    agendamento_id UUID PRIMARY KEY,
                    barbearia_id UUID NOT NULL,
                    cliente_id UUID NOT NULL,
                    barbeiro_id UUID NOT NULL,
                    servico_id UUID NOT NULL,
                    data_hora TIMESTAMP NOT NULL,
                    duracao_total INT NOT NULL,
                    status INT NOT NULL,
                    data_cancelamento TIMESTAMP NULL,
                    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
                    data_atualizacao TIMESTAMP NOT NULL DEFAULT NOW(),
                    CONSTRAINT fk_agendamentos_barbearia FOREIGN KEY (barbearia_id) REFERENCES barbershops(barbershop_id),
                    CONSTRAINT fk_agendamentos_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(cliente_id),
                    CONSTRAINT fk_agendamentos_barbeiro FOREIGN KEY (barbeiro_id) REFERENCES barbers(barber_id),
                    CONSTRAINT fk_agendamentos_servico FOREIGN KEY (servico_id) REFERENCES barbershop_services(service_id)
                );

                CREATE INDEX idx_agendamentos_barbearia ON agendamentos(barbearia_id);
                CREATE INDEX idx_agendamentos_barbeiro_data ON agendamentos(barbeiro_id, data_hora);
                CREATE INDEX idx_agendamentos_cliente_status ON agendamentos(cliente_id, status);
                CREATE INDEX idx_agendamentos_data_hora ON agendamentos(data_hora);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS agendamentos;
            ");
        }
    }
}
