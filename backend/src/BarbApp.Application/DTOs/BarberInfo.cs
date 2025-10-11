// BarbApp.Application/DTOs/BarberInfo.cs
namespace BarbApp.Application.DTOs;

public record BarberInfo
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid BarbeariaId { get; init; }
    public string NomeBarbearia { get; init; } = string.Empty;
}