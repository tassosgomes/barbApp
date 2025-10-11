// BarbApp.Domain/Exceptions/DomainException.cs (base)
namespace BarbApp.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}