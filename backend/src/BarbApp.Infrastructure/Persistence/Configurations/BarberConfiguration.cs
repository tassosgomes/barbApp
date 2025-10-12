// BarbApp.Infrastructure/Persistence/Configurations/BarberConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        // Primary key
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("barber_id");

        // Properties
        builder.Property(b => b.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(b => b.Telefone).HasColumnName("telefone").IsRequired().HasMaxLength(11);
        builder.Property(b => b.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
        builder.Property(b => b.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Indexes for performance
        builder.HasIndex(b => new { b.Telefone, b.BarbeariaId })
            .HasDatabaseName("ix_barbers_telefone_barbearia_id")
            .IsUnique();

        builder.HasIndex(b => b.BarbeariaId)
            .HasDatabaseName("ix_barbers_barbearia_id");

        builder.HasIndex(b => b.Telefone)
            .HasDatabaseName("ix_barbers_telefone");

        // Relationships
        builder.HasOne<Barbershop>()
            .WithMany()
            .HasForeignKey(b => b.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Table name
        builder.ToTable("barbers");
    }
}
