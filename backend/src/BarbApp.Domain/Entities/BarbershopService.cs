namespace BarbApp.Domain.Entities;

public class BarbershopService
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private BarbershopService()
    {
        Name = null!;
    }

    public static BarbershopService Create(
        Guid barbeariaId,
        string name,
        string? description,
        int durationMinutes,
        decimal price)
    {
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("Barbearia ID is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required");
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Service name must not exceed {MaxNameLength} characters");
        if (durationMinutes <= 0)
            throw new ArgumentException("Service duration must be greater than zero");
        if (price < 0)
            throw new ArgumentException("Service price must be zero or greater");
        if (description?.Length > MaxDescriptionLength)
            throw new ArgumentException($"Service description must not exceed {MaxDescriptionLength} characters");

        return new BarbershopService
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Name = name.Trim(),
            Description = description?.Trim(),
            DurationMinutes = durationMinutes,
            Price = price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        int durationMinutes,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required");
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Service name must not exceed {MaxNameLength} characters");
        if (durationMinutes <= 0)
            throw new ArgumentException("Service duration must be greater than zero");
        if (price < 0)
            throw new ArgumentException("Service price must be zero or greater");
        if (description?.Length > MaxDescriptionLength)
            throw new ArgumentException($"Service description must not exceed {MaxDescriptionLength} characters");

        Name = name.Trim();
        Description = description?.Trim();
        DurationMinutes = durationMinutes;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
