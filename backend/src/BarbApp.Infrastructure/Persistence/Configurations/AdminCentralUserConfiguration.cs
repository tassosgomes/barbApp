// BarbApp.Infrastructure/Persistence/Configurations/AdminCentralUserConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class AdminCentralUserConfiguration : IEntityTypeConfiguration<AdminCentralUser>
{
    public void Configure(EntityTypeBuilder<AdminCentralUser> builder)
    {
        builder.ToTable("admin_central_users");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("admin_central_user_id")
            .ValueGeneratedNever();

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

        // Índices
        builder.HasIndex(a => a.Email)
            .IsUnique()
            .HasDatabaseName("idx_admin_central_users_email");
    }
}