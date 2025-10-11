// BarbApp.Infrastructure/Persistence/Configurations/AdminBarbeariaUserConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class AdminBarbeariaUserConfiguration : IEntityTypeConfiguration<AdminBarbeariaUser>
{
    public void Configure(EntityTypeBuilder<AdminBarbeariaUser> builder)
    {
        builder.ToTable("admin_barbearia_users");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("admin_barbearia_user_id")
            .ValueGeneratedNever();

        builder.Property(a => a.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(a => a.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(a => a.Barbearia)
            .WithMany()
            .HasForeignKey(a => a.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices e constraints
        builder.HasIndex(a => new { a.BarbeariaId, a.Email })
            .IsUnique()
            .HasDatabaseName("idx_admin_barbearia_users_barbearia_email");

        builder.HasIndex(a => a.BarbeariaId)
            .HasDatabaseName("idx_admin_barbearia_users_barbearia_id");
    }
}