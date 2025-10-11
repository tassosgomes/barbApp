// BarbApp.Domain/Exceptions/ValidationException.cs
namespace BarbApp.Domain.Exceptions;

public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}