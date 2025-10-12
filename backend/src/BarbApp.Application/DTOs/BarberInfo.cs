// BarbApp.Application/DTOs/BarberInfo.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Informações de um barbeiro
/// </summary>
public record BarberInfo
{
    /// <summary>
    /// ID único do barbeiro
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome completo do barbeiro
    /// </summary>
    /// <example>João Silva</example>
    public string Nome { get; init; } = string.Empty;

    /// <summary>
    /// Telefone do barbeiro (apenas números)
    /// </summary>
    /// <example>11987654321</example>
    public string Telefone { get; init; } = string.Empty;

    /// <summary>
    /// ID da barbearia onde o barbeiro trabalha
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid BarbeariaId { get; init; }

    /// <summary>
    /// Nome da barbearia onde o barbeiro trabalha
    /// </summary>
    /// <example>Barbearia Premium</example>
    public string NomeBarbearia { get; init; } = string.Empty;
}