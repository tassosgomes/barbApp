// BarbApp.Application/DTOs/LoginAdminCentralInput.cs
namespace BarbApp.Application.DTOs;

public record LoginAdminCentralInput
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}