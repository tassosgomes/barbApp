// BarbApp.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("customer_id")
            .ValueGeneratedNever();

        builder.Property(c => c.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(c => c.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(c => c.Barbearia)
            .WithMany()
            .HasForeignKey(c => c.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices e constraints
        builder.HasIndex(c => new { c.BarbeariaId, c.Telefone })
            .IsUnique()
            .HasDatabaseName("idx_customers_barbearia_telefone");

        builder.HasIndex(c => c.Telefone)
            .HasDatabaseName("idx_customers_telefone");
    }
}