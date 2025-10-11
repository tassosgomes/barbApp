// BarbApp.Domain/Entities/Barbershop.cs
using BarbApp.Domain.ValueObjects;

namespace BarbApp.Domain.Entities;

public class Barbershop
{
    public Guid Id { get; private set; }
    public BarbeariaCode Code { get; private set; } = null!;
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<AdminBarbeariaUser> AdminUsers { get; private set; } = new List<AdminBarbeariaUser>();
    public ICollection<Barber> Barbers { get; private set; } = new List<Barber>();
    public ICollection<Customer> Customers { get; private set; } = new List<Customer>();

    private Barbershop() { } // EF Core

    public static Barbershop Create(string name, BarbeariaCode code)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new Barbershop
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}