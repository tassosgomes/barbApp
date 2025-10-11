// BarbApp.Domain/Entities/AdminCentralUser.cs
namespace BarbApp.Domain.Entities;

public class AdminCentralUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AdminCentralUser() { } // EF Core

    public static AdminCentralUser Create(
        string email,
        string passwordHash,
        string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new AdminCentralUser
        {
            Id = Guid.NewGuid(),
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

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}