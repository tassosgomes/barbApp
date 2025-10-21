using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class LandingPageServiceConfiguration : IEntityTypeConfiguration<LandingPageService>
{
    public void Configure(EntityTypeBuilder<LandingPageService> builder)
    {
        builder.ToTable("landing_page_services");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("landing_page_service_id")
            .ValueGeneratedNever();

        builder.Property(l => l.LandingPageConfigId)
            .HasColumnName("landing_page_config_id")
            .IsRequired();

        builder.Property(l => l.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(l => l.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(l => l.LandingPageConfig)
            .WithMany(lp => lp.Services)
            .HasForeignKey(l => l.LandingPageConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Service)
            .WithMany()
            .HasForeignKey(l => l.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.LandingPageConfigId)
            .HasDatabaseName("ix_landing_page_services_config_id");

        builder.HasIndex(l => l.ServiceId)
            .HasDatabaseName("ix_landing_page_services_service_id");

        builder.HasIndex(l => new { l.LandingPageConfigId, l.DisplayOrder })
            .HasDatabaseName("ix_landing_page_services_config_order");

        builder.HasIndex(l => new { l.LandingPageConfigId, l.ServiceId })
            .IsUnique()
            .HasDatabaseName("uq_landing_page_services_config_service");
    }
}
