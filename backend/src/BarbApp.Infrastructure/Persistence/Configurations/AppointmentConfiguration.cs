// BarbApp.Infrastructure/Persistence/Configurations/AppointmentConfiguration.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        // Table name
        builder.ToTable("appointments");

        // Primary key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("appointment_id");

        // Properties
        builder.Property(a => a.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(a => a.BarberId).HasColumnName("barber_id").IsRequired();
        builder.Property(a => a.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(a => a.ServiceId).HasColumnName("service_id").IsRequired();
        builder.Property(a => a.StartTime).HasColumnName("start_time").IsRequired();
        builder.Property(a => a.EndTime).HasColumnName("end_time").IsRequired();
        
        // Status as enum stored as string
        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        // Timestamps
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(a => a.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(a => a.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(a => a.CompletedAt).HasColumnName("completed_at");

        // Ignore computed properties
        builder.Ignore(a => a.ServiceName);
        builder.Ignore(a => a.StatusString);

        // Indexes for performance
        builder.HasIndex(a => a.BarbeariaId)
            .HasDatabaseName("ix_appointments_barbearia_id");

        builder.HasIndex(a => a.BarberId)
            .HasDatabaseName("ix_appointments_barber_id");

        builder.HasIndex(a => a.CustomerId)
            .HasDatabaseName("ix_appointments_customer_id");

        builder.HasIndex(a => new { a.BarbeariaId, a.StartTime })
            .HasDatabaseName("ix_appointments_barbearia_start_time");

        builder.HasIndex(a => a.StartTime)
            .HasDatabaseName("ix_appointments_start_time");

        builder.HasIndex(a => new { a.BarberId, a.StartTime })
            .HasDatabaseName("ix_appointments_barber_start_time");

        // Navigation properties
        builder.HasOne(a => a.Barbearia)
            .WithMany()
            .HasForeignKey(a => a.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Barber)
            .WithMany()
            .HasForeignKey(a => a.BarberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}