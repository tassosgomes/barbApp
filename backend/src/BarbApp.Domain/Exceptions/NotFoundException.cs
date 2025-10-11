// BarbApp.Domain/Exceptions/NotFoundException.cs
using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message) { }
}