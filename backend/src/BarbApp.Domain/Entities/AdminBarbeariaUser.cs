// BarbApp.Domain/Entities/AdminBarbeariaUser.cs
namespace BarbApp.Domain.Entities;

public class AdminBarbeariaUser
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AdminBarbeariaUser()
    {
        Email = null!;
        PasswordHash = null!;
        Name = null!;
    }

    public static AdminBarbeariaUser Create(
        Guid barbeariaId,
        string email,
        string passwordHash,
        string name)
    {
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("Barbearia ID is required");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new AdminBarbeariaUser
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Name = name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool VerifyPassword(string passwordHash)
    {
        return PasswordHash == passwordHash;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}