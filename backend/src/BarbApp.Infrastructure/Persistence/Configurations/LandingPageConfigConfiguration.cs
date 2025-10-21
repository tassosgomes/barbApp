using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class LandingPageConfigConfiguration : IEntityTypeConfiguration<LandingPageConfig>
{
    public void Configure(EntityTypeBuilder<LandingPageConfig> builder)
    {
        builder.ToTable("landing_page_configs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("landing_page_config_id")
            .ValueGeneratedNever();

        builder.Property(l => l.BarbershopId)
            .HasColumnName("barbershop_id")
            .IsRequired();

        builder.Property(l => l.TemplateId)
            .HasColumnName("template_id")
            .IsRequired();

        builder.Property(l => l.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(l => l.AboutText)
            .HasColumnName("about_text")
            .HasMaxLength(2000);

        builder.Property(l => l.OpeningHours)
            .HasColumnName("opening_hours")
            .HasMaxLength(500);

        builder.Property(l => l.InstagramUrl)
            .HasColumnName("instagram_url")
            .HasMaxLength(255);

        builder.Property(l => l.FacebookUrl)
            .HasColumnName("facebook_url")
            .HasMaxLength(255);

        builder.Property(l => l.WhatsappNumber)
            .HasColumnName("whatsapp_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.IsPublished)
            .HasColumnName("is_published")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(l => l.Barbershop)
            .WithMany()
            .HasForeignKey(l => l.BarbershopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.BarbershopId)
            .HasDatabaseName("ix_landing_page_configs_barbershop_id");

        builder.HasIndex(l => l.IsPublished)
            .HasDatabaseName("ix_landing_page_configs_is_published");

        builder.HasIndex(l => l.BarbershopId)
            .IsUnique()
            .HasDatabaseName("uq_landing_page_configs_barbershop");
    }
}
