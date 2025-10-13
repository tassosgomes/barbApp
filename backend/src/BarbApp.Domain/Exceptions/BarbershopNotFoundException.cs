namespace BarbApp.Domain.Exceptions;

public class BarbershopNotFoundException : NotFoundException
{
    public BarbershopNotFoundException(string message) : base(message) { }
}