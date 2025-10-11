// BarbApp.Domain/Exceptions/UnauthorizedException.cs
namespace BarbApp.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message) { }
}