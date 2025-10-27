// BarbApp.Application/DTOs/BarbeariaDto.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados simplificados de barbearia para respostas da API
/// </summary>
public record BarbeariaDto
{
    /// <summary>
    /// ID único da barbearia
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome da barbearia
    /// </summary>
    public string Nome { get; init; } = string.Empty;

    /// <summary>
    /// Código único da barbearia (8 caracteres)
    /// </summary>
    public string Codigo { get; init; } = string.Empty;
}