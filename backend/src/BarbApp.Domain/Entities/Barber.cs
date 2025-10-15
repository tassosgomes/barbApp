using System.Text.RegularExpressions;

namespace BarbApp.Domain.Entities;

public class Barber
{
    private const int MaxNameLength = 100;
    private const int MaxEmailLength = 255;
    private const int BrazilCountryCode = 55;
    private const int PhoneWithCountryCodeLength = 13;
    private const int CountryCodeDigits = 2;
    private const int MinPhoneDigits = 10;
    private const int MaxPhoneDigits = 11;
    private const string EmailValidationPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Phone { get; private set; }
    public List<Guid> ServiceIds { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Barber()
    {
        Name = null!;
        Email = null!;
        PasswordHash = null!;
        Phone = null!;
        ServiceIds = new List<Guid>();
    }

    public static Barber Create(
        Guid barbeariaId,
        string name,
        string email,
        string passwordHash,
        string phone,
        List<Guid>? serviceIds = null)
    {
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("Barbearia ID is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name must not exceed {MaxNameLength} characters");
        
        ValidateEmail(email);
        ValidatePasswordHash(passwordHash);
        
        var cleanedPhone = CleanAndValidatePhone(phone);

        return new Barber
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Name = name.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Phone = cleanedPhone,
            ServiceIds = serviceIds ?? new List<Guid>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string phone, List<Guid>? serviceIds = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name must not exceed {MaxNameLength} characters");
        
        var cleanedPhone = CleanAndValidatePhone(phone);

        Name = name.Trim();
        Phone = cleanedPhone;
        ServiceIds = serviceIds ?? new List<Guid>();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(string newEmail)
    {
        ValidateEmail(newEmail);
        Email = newEmail.ToLowerInvariant().Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        ValidatePasswordHash(newPasswordHash);
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");
        if (email.Length > MaxEmailLength)
            throw new ArgumentException($"Email must not exceed {MaxEmailLength} characters");
        
        if (!Regex.IsMatch(email, EmailValidationPattern))
            throw new ArgumentException("Invalid email format");
    }

    private static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");
    }

    private static string CleanAndValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required");

        var cleaned = Regex.Replace(phone, @"[^\d]", "");

        if (cleaned.StartsWith(BrazilCountryCode.ToString()) && cleaned.Length == PhoneWithCountryCodeLength)
            cleaned = cleaned.Substring(CountryCodeDigits);

        if (!Regex.IsMatch(cleaned, $@"^\d{{{MinPhoneDigits},{MaxPhoneDigits}}}$"))
            throw new ArgumentException($"Phone must contain {MinPhoneDigits} or {MaxPhoneDigits} digits (Brazilian format)");

        return cleaned;
    }
}