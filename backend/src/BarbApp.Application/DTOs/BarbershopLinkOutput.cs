namespace BarbApp.Application.DTOs;

/// <summary>
/// Vinculação do barbeiro a uma barbearia (para seleção de contexto)
/// </summary>
public record BarbershopLinkOutput(
    Guid Id,
    string Nome,
    string Codigo,
    bool IsActive
);

