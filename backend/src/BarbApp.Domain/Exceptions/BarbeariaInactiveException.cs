// BarbApp.Domain/Exceptions/BarbeariaInactiveException.cs
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Exceptions;

public class BarbeariaInactiveException : DomainException
{
    public BarbeariaInactiveException(string message)
        : base(message) { }
}