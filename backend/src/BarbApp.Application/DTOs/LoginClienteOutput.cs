// BarbApp.Application/DTOs/LoginClienteOutput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de saída para resposta de login de cliente
/// </summary>
public record LoginClienteOutput
{
    /// <summary>
    /// Token JWT para autenticação
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Dados do cliente autenticado
    /// </summary>
    public ClienteDto Cliente { get; init; } = new();

    /// <summary>
    /// Dados da barbearia onde o cliente está logado
    /// </summary>
    public BarbeariaDto Barbearia { get; init; } = new();
}