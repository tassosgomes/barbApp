// BarbApp.Domain/Exceptions/ForbiddenException.cs
namespace BarbApp.Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}