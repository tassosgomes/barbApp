// BarbApp.Application/DTOs/CadastroClienteOutput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de saída para resposta de cadastro de cliente
/// </summary>
public record CadastroClienteOutput
{
    /// <summary>
    /// Token JWT para autenticação
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Dados do cliente recém-cadastrado
    /// </summary>
    public ClienteDto Cliente { get; init; } = new();

    /// <summary>
    /// Dados da barbearia onde o cliente foi cadastrado
    /// </summary>
    public BarbeariaDto Barbearia { get; init; } = new();
}