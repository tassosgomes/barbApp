// BarbApp.Infrastructure/Services/PasswordHasher.cs
using BarbApp.Application.Interfaces;

namespace BarbApp.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Password cannot be null or whitespace",
                nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Password cannot be null or whitespace",
                nameof(password));
        }

        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException(
                "Password hash cannot be null or whitespace",
                nameof(hash));
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}