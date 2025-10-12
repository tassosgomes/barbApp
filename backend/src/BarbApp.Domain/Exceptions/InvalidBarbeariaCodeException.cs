// BarbApp.Domain/Exceptions/InvalidUniqueCodeException.cs
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Exceptions;

public class InvalidUniqueCodeException : DomainException
{
    public InvalidUniqueCodeException(string message)
        : base(message) { }
}