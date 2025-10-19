namespace BarbApp.Application.DTOs;

/// <summary>
/// DTO para retorno dos dados de perfil do barbeiro autenticado
/// </summary>
public record BarberProfileOutput
{
    /// <summary>
    /// ID do barbeiro
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome do barbeiro
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// E-mail do barbeiro
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Telefone do barbeiro
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Indica se o barbeiro está ativo
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// ID da barbearia à qual o barbeiro pertence
    /// </summary>
    public Guid BarbeariaId { get; init; }

    /// <summary>
    /// Nome da barbearia
    /// </summary>
    public string BarbeariaNome { get; init; } = string.Empty;

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
