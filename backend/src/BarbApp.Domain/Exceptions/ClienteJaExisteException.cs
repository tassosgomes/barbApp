// BarbApp.Domain/Exceptions/ClienteJaExisteException.cs
namespace BarbApp.Domain.Exceptions;

public class ClienteJaExisteException : DomainException
{
    public ClienteJaExisteException(string telefone)
        : base($"Cliente com telefone '{telefone}' já está cadastrado nesta barbearia")
    {
    }
}