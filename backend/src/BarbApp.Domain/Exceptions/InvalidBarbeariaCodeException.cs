// BarbApp.Domain/Exceptions/InvalidBarbeariaCodeException.cs
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Exceptions;

public class InvalidBarbeariaCodeException : DomainException
{
    public InvalidBarbeariaCodeException(string message)
        : base(message) { }
}