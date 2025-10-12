// BarbApp.Domain/Entities/Barber.cs
using System.Text.RegularExpressions;

namespace BarbApp.Domain.Entities;

public class Barber
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
    public string Telefone { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Barber() 
    {
        Telefone = null!;
        Name = null!;
    }

    public static Barber Create(
        Guid barbeariaId,
        string telefone,
        string name)
    {
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("Barbearia ID is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new Barber
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Telefone = CleanAndValidatePhone(telefone),
            Name = name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string CleanAndValidatePhone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone is required");

        var cleaned = Regex.Replace(telefone, @"[^\d]", "");

        // Remover prefixo +55 se presente
        if (cleaned.StartsWith("55") && cleaned.Length == 13)
            cleaned = cleaned.Substring(2);

        // Validar formato brasileiro: 10 ou 11 dígitos (DDD + número)
        if (!Regex.IsMatch(cleaned, @"^\d{10,11}$"))
            throw new ArgumentException("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");

        return cleaned;
    }
}