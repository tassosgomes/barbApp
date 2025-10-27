// BarbApp.Domain/Exceptions/BarbeariaNotFoundException.cs
namespace BarbApp.Domain.Exceptions;

public class BarbeariaNotFoundException : NotFoundException
{
    public BarbeariaNotFoundException(string codigo)
        : base($"Barbearia com código '{codigo}' não encontrada")
    {
    }
}