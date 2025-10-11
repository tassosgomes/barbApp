// BarbApp.Application/DTOs/TrocarContextoInput.cs
namespace BarbApp.Application.DTOs;

public record TrocarContextoInput
{
    public Guid NovaBarbeariaId { get; init; }
}