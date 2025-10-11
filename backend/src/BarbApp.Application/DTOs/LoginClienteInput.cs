// BarbApp.Application/DTOs/LoginClienteInput.cs
namespace BarbApp.Application.DTOs;

public record LoginClienteInput
{
    public string Codigo { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
}