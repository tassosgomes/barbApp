// BarbApp.Domain/ValueObjects/BarbeariaCode.cs
using System.Text.RegularExpressions;
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.ValueObjects;

public class BarbeariaCode
{
    public string Value { get; private set; }

    private BarbeariaCode() { } // EF Core

    public static BarbeariaCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidBarbeariaCodeException("Code cannot be empty");

        var upperValue = value.ToUpperInvariant().Trim();

        if (upperValue.Length != 8)
            throw new InvalidBarbeariaCodeException("Code must be 8 characters");

        if (!Regex.IsMatch(upperValue, @"^[A-Z2-9]{8}$"))
            throw new InvalidBarbeariaCodeException(
                "Code must contain only uppercase letters and numbers (excluding O, I, 0, 1)");

        return new BarbeariaCode { Value = upperValue };
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) =>
        obj is BarbeariaCode other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(BarbeariaCode? left, BarbeariaCode? right) =>
        left is null && right is null || left is not null && right is not null && left.Value == right.Value;

    public static bool operator !=(BarbeariaCode? left, BarbeariaCode? right) =>
        !(left == right);
}