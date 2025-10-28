namespace BarbApp.Domain.Exceptions;

public class AgendamentoJaExisteException : DomainException
{
    public AgendamentoJaExisteException(string message) : base(message)
    {
    }
}