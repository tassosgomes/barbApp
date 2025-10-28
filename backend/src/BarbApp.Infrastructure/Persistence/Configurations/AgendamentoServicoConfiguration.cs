// BarbApp.Infrastructure/Persistence/Configurations/AgendamentoServicoConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class AgendamentoServicoConfiguration : IEntityTypeConfiguration<AgendamentoServico>
{
    public void Configure(EntityTypeBuilder<AgendamentoServico> builder)
    {
        builder.ToTable("agendamento_servicos");

        builder.HasKey(ag => new { ag.AgendamentoId, ag.ServicoId });

        builder.Property(ag => ag.AgendamentoId).HasColumnName("agendamento_id").IsRequired();
        builder.Property(ag => ag.ServicoId).HasColumnName("servico_id").IsRequired();

        // Relacionamentos
        builder.HasOne(ag => ag.Agendamento)
            .WithMany(a => a.AgendamentoServicos)
            .HasForeignKey(ag => ag.AgendamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ag => ag.Servico)
            .WithMany()
            .HasForeignKey(ag => ag.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(ag => ag.AgendamentoId).HasDatabaseName("idx_agendamento_servicos_agendamento");
        builder.HasIndex(ag => ag.ServicoId).HasDatabaseName("idx_agendamento_servicos_servico");
    }
}