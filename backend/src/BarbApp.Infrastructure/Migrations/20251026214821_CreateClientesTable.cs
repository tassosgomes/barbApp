using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateClientesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE clientes (
                    cliente_id UUID PRIMARY KEY,
                    barbearia_id UUID NOT NULL,
                    nome VARCHAR(200) NOT NULL,
                    telefone VARCHAR(11) NOT NULL,
                    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
                    data_atualizacao TIMESTAMP NOT NULL DEFAULT NOW(),
                    CONSTRAINT fk_clientes_barbearia FOREIGN KEY (barbearia_id) REFERENCES barbershops(barbershop_id),
                    CONSTRAINT uk_clientes_telefone_barbearia UNIQUE (telefone, barbearia_id)
                );

                CREATE INDEX idx_clientes_barbearia ON clientes(barbearia_id);
                CREATE INDEX idx_clientes_telefone_barbearia ON clientes(telefone, barbearia_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS clientes;
            ");
        }
    }
}
