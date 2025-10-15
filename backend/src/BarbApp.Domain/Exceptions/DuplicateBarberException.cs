namespace BarbApp.Domain.Exceptions;

public class DuplicateBarberException : DomainException
{
    public DuplicateBarberException(string email, Guid barbeariaId)
        : base($"A barber with email '{email}' already exists in Barbearia '{barbeariaId}'")
    {
    }
}
