
using System.Text.RegularExpressions;
using BarbApp.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Domain.ValueObjects
{
    [Owned]
    public class Document
    {
        public string Value { get; private set; }
        public DocumentType Type { get; private set; }

        private Document() 
        {
            Value = null!;
        } // EF Core

        public static Document Create(string value)
        {
            var cleanValue = CleanDocument(value);

            if (IsCnpj(cleanValue))
                return new Document { Value = cleanValue, Type = DocumentType.CNPJ };

            if (IsCpf(cleanValue))
                return new Document { Value = cleanValue, Type = DocumentType.CPF };

            throw new InvalidDocumentException("Invalid CNPJ or CPF format");
        }

        private static string CleanDocument(string value)
        {
            return Regex.Replace(value, @"[^\d]", "");
        }

        private static bool IsCnpj(string value)
        {            
            return value.Length == 14 && Regex.IsMatch(value, @"^\d{14}$");
        }

        private static bool IsCpf(string value)
        {
            return value.Length == 11 && Regex.IsMatch(value, @"^\d{11}$");
        }
    }

    public enum DocumentType
    {
        CPF = 1,
        CNPJ = 2
    }
}
