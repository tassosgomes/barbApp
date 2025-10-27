// BarbApp.Application/DTOs/ClienteDto.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados simplificados de cliente para respostas da API
/// </summary>
public record ClienteDto
{
    /// <summary>
    /// ID único do cliente
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome completo do cliente
    /// </summary>
    public string Nome { get; init; } = string.Empty;

    /// <summary>
    /// Telefone formatado do cliente
    /// </summary>
    public string Telefone { get; init; } = string.Empty;
}