namespace BarbApp.Domain.Exceptions;

public class HorarioIndisponivelException : DomainException
{
    public HorarioIndisponivelException(string message) : base(message)
    {
    }
}