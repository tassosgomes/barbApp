// BarbApp.Infrastructure/Persistence/Configurations/ClienteConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("cliente_id");
        builder.Property(c => c.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(c => c.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Telefone).HasColumnName("telefone").HasMaxLength(11).IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("data_criacao").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("data_atualizacao").IsRequired();

        // Global Query Filter para multi-tenancy
        builder.HasQueryFilter(c => c.BarbeariaId == Guid.Empty); // Will be replaced by runtime filter

        // Índices e constraints
        builder.HasIndex(c => c.BarbeariaId).HasDatabaseName("idx_clientes_barbearia");
        builder.HasIndex(c => new { c.Telefone, c.BarbeariaId })
            .HasDatabaseName("idx_clientes_telefone_barbearia")
            .IsUnique();

        // Relacionamentos
        builder.HasOne(c => c.Barbearia)
            .WithMany()
            .HasForeignKey(c => c.BarbeariaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}