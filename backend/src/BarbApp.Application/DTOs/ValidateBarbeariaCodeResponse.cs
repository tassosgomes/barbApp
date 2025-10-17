namespace BarbApp.Application.DTOs;

/// <summary>
/// Response retornado ao validar código de uma barbearia.
/// Contém apenas dados básicos e públicos, sem informações sensíveis.
/// </summary>
public record ValidateBarbeariaCodeResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
