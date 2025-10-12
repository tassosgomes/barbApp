// BarbApp.Infrastructure/Persistence/Configurations/BarbershopConfiguration.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.OwnsOne(b => b.Document, doc =>
        {
            doc.Property(d => d.Value)
                .HasColumnName("document")
                .HasMaxLength(14)
                .IsRequired();
            doc.Property(d => d.Type)
                .HasColumnName("document_type")
                .IsRequired();
            doc.HasIndex(d => d.Value).IsUnique();
        });

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

        builder.OwnsOne(b => b.Code, code =>
        {
            code.Property(c => c.Value)
                .HasColumnName("code")
                .HasMaxLength(8)
                .IsRequired();
            code.HasIndex(c => c.Value).IsUnique();
        });

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.AddressId).HasColumnName("address_id");

        builder.HasOne(b => b.Address)
            .WithMany()
            .HasForeignKey(b => b.AddressId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex("Code_Value")
            .IsUnique()
            .HasDatabaseName("uk_barbershops_code");

        builder.HasIndex("Document_Value")
            .IsUnique()
            .HasDatabaseName("uk_barbershops_document");

        builder.HasIndex(b => b.Name)
            .HasDatabaseName("idx_barbershops_name");

        builder.HasIndex(b => b.IsActive)
            .HasDatabaseName("idx_barbershops_is_active");
    }
}