using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class BarbershopServiceConfiguration : IEntityTypeConfiguration<BarbershopService>
{
    public void Configure(EntityTypeBuilder<BarbershopService> builder)
    {
        builder.ToTable("barbershop_services");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("service_id")
            .ValueGeneratedNever();

        builder.Property(s => s.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.DurationMinutes)
            .HasColumnName("duration_minutes")
            .IsRequired();

        builder.Property(s => s.Price)
            .HasColumnName("price")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(s => s.Barbearia)
            .WithMany()
            .HasForeignKey(s => s.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.BarbeariaId)
            .HasDatabaseName("ix_barbershop_services_barbearia_id");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("ix_barbershop_services_is_active");

        builder.HasIndex(s => new { s.BarbeariaId, s.Name })
            .HasDatabaseName("ix_barbershop_services_barbearia_name");
    }
}
