namespace BarbApp.Domain.Exceptions;

public class BarberNotFoundException : NotFoundException
{
    public BarberNotFoundException(Guid barberId)
        : base($"Barber with ID '{barberId}' was not found")
    {
    }

    public BarberNotFoundException(string email, Guid barbeariaId)
        : base($"Barber with email '{email}' in Barbearia '{barbeariaId}' was not found")
    {
    }
}
