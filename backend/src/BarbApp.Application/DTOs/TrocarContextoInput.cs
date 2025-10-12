// BarbApp.Application/DTOs/TrocarContextoInput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Dados de entrada para troca de contexto de barbearia
/// </summary>
public record TrocarContextoInput
{
    /// <summary>
    /// ID da nova barbearia para trocar o contexto
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid NovaBarbeariaId { get; init; }
}