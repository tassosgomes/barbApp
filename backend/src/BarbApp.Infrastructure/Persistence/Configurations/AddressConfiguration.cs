// BarbApp.Infrastructure/Persistence/Configurations/AddressConfiguration.cs
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("address_id")
            .ValueGeneratedNever();

        builder.Property(a => a.ZipCode)
            .HasColumnName("zip_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(a => a.Street)
            .HasColumnName("street")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Number)
            .HasColumnName("number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Complement)
            .HasColumnName("complement")
            .HasMaxLength(255);

        builder.Property(a => a.Neighborhood)
            .HasColumnName("neighborhood")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.City)
            .HasColumnName("city")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.State)
            .HasColumnName("state")
            .HasMaxLength(2)
            .IsRequired();
    }
}