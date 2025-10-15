// BarbApp.Infrastructure/Persistence/Configurations/AppointmentConfiguration.cs
using BarbApp.Domain.Entities;
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
        builder.Property(a => a.ServiceName).HasColumnName("service_name").IsRequired().HasMaxLength(100);
        builder.Property(a => a.Status).HasColumnName("status").IsRequired().HasMaxLength(20);

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

        // Relationships
        builder.HasOne<Barbershop>()
            .WithMany()
            .HasForeignKey(a => a.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Barber>()
            .WithMany()
            .HasForeignKey(a => a.BarberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}