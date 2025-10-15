// BarbApp.Infrastructure/Persistence/Configurations/BarberConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        // Table name
        builder.ToTable("barbers");

        // Primary key
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("barber_id");

        // Properties
        builder.Property(b => b.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(b => b.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(b => b.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        builder.Property(b => b.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(b => b.Phone).HasColumnName("phone").IsRequired().HasMaxLength(11);
        builder.Property(b => b.ServiceIds).HasColumnName("service_ids").IsRequired();
        builder.Property(b => b.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Indexes for performance
        builder.HasIndex(b => new { b.BarbeariaId, b.Email })
            .HasDatabaseName("uq_barbers_barbearia_email")
            .IsUnique();

        builder.HasIndex(b => b.BarbeariaId)
            .HasDatabaseName("ix_barbers_barbearia_id");

        builder.HasIndex(b => b.Email)
            .HasDatabaseName("ix_barbers_email");

        builder.HasIndex(b => b.Phone)
            .HasDatabaseName("ix_barbers_phone");

        builder.HasIndex(b => new { b.BarbeariaId, b.IsActive })
            .HasDatabaseName("ix_barbers_barbearia_is_active");

        // Relationships
        builder.HasOne(b => b.Barbearia)
            .WithMany()
            .HasForeignKey(b => b.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
