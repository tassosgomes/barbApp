// BarbApp.Infrastructure/Persistence/Configurations/AgendamentoConfiguration.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.ToTable("agendamentos");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("agendamento_id");
        builder.Property(a => a.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(a => a.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(a => a.BarbeiroId).HasColumnName("barbeiro_id").IsRequired();

        // Removed single service ID - now using many-to-many relationship

        builder.Property(a => a.DataHora).HasColumnName("data_hora").IsRequired();
        builder.Property(a => a.DuracaoMinutos).HasColumnName("duracao_total").IsRequired();
        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(a => a.DataCancelamento).HasColumnName("data_cancelamento");
        builder.Property(a => a.CreatedAt).HasColumnName("data_criacao").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("data_atualizacao").IsRequired();

        // Global Query Filter para multi-tenancy
        builder.HasQueryFilter(a => a.BarbeariaId == Guid.Empty); // Will be replaced by runtime filter

        // Índices
        builder.HasIndex(a => a.BarbeariaId).HasDatabaseName("idx_agendamentos_barbearia");
        builder.HasIndex(a => new { a.BarbeiroId, a.DataHora })
            .HasDatabaseName("idx_agendamentos_barbeiro_data");
        builder.HasIndex(a => new { a.ClienteId, a.Status })
            .HasDatabaseName("idx_agendamentos_cliente_status");
        builder.HasIndex(a => a.DataHora).HasDatabaseName("idx_agendamentos_data_hora");

        // Relacionamentos
        builder.HasOne(a => a.Cliente)
            .WithMany(c => c.Agendamentos)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Barbeiro)
            .WithMany()
            .HasForeignKey(a => a.BarbeiroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Barbearia)
            .WithMany()
            .HasForeignKey(a => a.BarbeariaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-many relationship with services
        builder.HasMany(a => a.AgendamentoServicos)
            .WithOne(ag => ag.Agendamento)
            .HasForeignKey(ag => ag.AgendamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}