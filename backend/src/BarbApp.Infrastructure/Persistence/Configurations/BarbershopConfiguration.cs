// BarbApp.Infrastructure/Persistence/Configurations/BarbershopConfiguration.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class BarbershopConfiguration : IEntityTypeConfiguration<Barbershop>
{
    public void Configure(EntityTypeBuilder<Barbershop> builder)
    {
        builder.ToTable("barbershops");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("barbershop_id")
            .ValueGeneratedNever();

        // Value object conversion
        builder.Property(b => b.Code)
            .HasColumnName("code")
            .HasMaxLength(8)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => BarbeariaCode.Create(v));

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

        // Índices
        builder.HasIndex(b => b.Code)
            .IsUnique()
            .HasDatabaseName("idx_barbershops_code");
    }
}