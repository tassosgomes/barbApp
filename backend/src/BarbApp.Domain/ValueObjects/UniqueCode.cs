
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Domain.ValueObjects
{
    [Owned]
    public class UniqueCode
    {
        [Index(IsUnique = true)]
        public string Value { get; private set; }

        private UniqueCode() 
        {
            Value = null!;
        } // EF Core

        public static UniqueCode Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Code cannot be empty");

            var upperValue = value.ToUpperInvariant();

            if (upperValue.Length != 8)
                throw new ArgumentException("Code must be 8 characters");

            if (!Regex.IsMatch(upperValue, @"^[A-Z2-9]{8}$"))
                throw new ArgumentException("Code must contain only uppercase letters and numbers (excluding O, I, 0, 1)");

            return new UniqueCode { Value = upperValue };
        }
    }
}
