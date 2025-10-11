// BarbApp.Domain/Exceptions/UnauthorizedAccessException.cs
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Exceptions;

public class UnauthorizedAccessException : DomainException
{
    public UnauthorizedAccessException(string message)
        : base(message) { }
}