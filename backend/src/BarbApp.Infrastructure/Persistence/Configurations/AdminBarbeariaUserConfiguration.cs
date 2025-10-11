// BarbApp.Infrastructure/Persistence/Configurations/AdminBarbeariaUserConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class AdminBarbeariaUserConfiguration : IEntityTypeConfiguration<AdminBarbeariaUser>
{
    public void Configure(EntityTypeBuilder<AdminBarbeariaUser> builder)
    {
        // Primary key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("admin_barbearia_user_id");

        // Properties
        builder.Property(a => a.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(a => a.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        builder.Property(a => a.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(a => a.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
        builder.Property(a => a.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Indexes for performance
        builder.HasIndex(a => new { a.Email, a.BarbeariaId })
            .HasDatabaseName("ix_admin_barbearia_users_email_barbearia_id")
            .IsUnique();

        builder.HasIndex(a => a.BarbeariaId)
            .HasDatabaseName("ix_admin_barbearia_users_barbearia_id");

        builder.HasIndex(a => a.Email)
            .HasDatabaseName("ix_admin_barbearia_users_email");

        // Relationships
        builder.HasOne(a => a.Barbearia)
            .WithMany(b => b.AdminUsers)
            .HasForeignKey(a => a.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Table name
        builder.ToTable("admin_barbearia_users");
    }
}
