// BarbApp.Infrastructure/Persistence/Configurations/BarberConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        builder.ToTable("barbers");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("barber_id")
            .ValueGeneratedNever();

        builder.Property(b => b.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(b => b.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(b => b.Barbearia)
            .WithMany()
            .HasForeignKey(b => b.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices e constraints
        builder.HasIndex(b => new { b.BarbeariaId, b.Telefone })
            .IsUnique()
            .HasDatabaseName("idx_barbers_barbearia_telefone");

        builder.HasIndex(b => b.Telefone)
            .HasDatabaseName("idx_barbers_telefone");
    }
}