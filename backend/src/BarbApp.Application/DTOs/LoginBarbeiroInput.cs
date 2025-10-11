// BarbApp.Application/DTOs/LoginBarbeiroInput.cs
namespace BarbApp.Application.DTOs;

public record LoginBarbeiroInput
{
    public string Codigo { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
}