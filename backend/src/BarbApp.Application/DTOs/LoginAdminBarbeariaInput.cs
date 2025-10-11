// BarbApp.Application/DTOs/LoginAdminBarbeariaInput.cs
namespace BarbApp.Application.DTOs;

public record LoginAdminBarbeariaInput
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
}