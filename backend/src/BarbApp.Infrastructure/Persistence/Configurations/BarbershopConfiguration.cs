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
                v => UniqueCode.Create(v));

        builder.Property(b => b.Document)
            .HasColumnName("document")
            .HasMaxLength(14)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => Document.Create(v));

        builder.Property(b => b.Phone)
            .HasColumnName("phone")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(b => b.OwnerName)
            .HasColumnName("owner_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255)
            .IsRequired();

        // Address relationship
        builder.HasOne(b => b.Address)
            .WithOne()
            .HasForeignKey<Barbershop>(b => b.AddressId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(b => b.Code)
            .IsUnique()
            .HasDatabaseName("idx_barbershops_code");
    }
}