// BarbApp.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("customer_id");

        // Properties
        builder.Property(c => c.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(c => c.Telefone).HasColumnName("telefone").IsRequired().HasMaxLength(11);
        builder.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
        builder.Property(c => c.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Indexes for performance
        builder.HasIndex(c => new { c.Telefone, c.BarbeariaId })
            .HasDatabaseName("ix_customers_telefone_barbearia_id")
            .IsUnique();

        builder.HasIndex(c => c.BarbeariaId)
            .HasDatabaseName("ix_customers_barbearia_id");

        builder.HasIndex(c => c.Telefone)
            .HasDatabaseName("ix_customers_telefone");

        // Relationships
        builder.HasOne(c => c.Barbearia)
            .WithMany()
            .HasForeignKey(c => c.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Table name
        builder.ToTable("customers");
    }
}
