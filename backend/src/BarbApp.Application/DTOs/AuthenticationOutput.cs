// BarbApp.Application/DTOs/AuthenticationOutput.cs
namespace BarbApp.Application.DTOs;

/// <summary>
/// Saída de operação de autenticação
/// </summary>
/// <param name="UserId">ID único do usuário autenticado</param>
/// <param name="Name">Nome do usuário autenticado</param>
/// <param name="Role">Papel do usuário (AdminCentral, AdminBarbearia, Barbeiro, Cliente)</param>
/// <param name="BarbeariaId">ID da barbearia (null para AdminCentral)</param>
/// <param name="BarbeariaCode">Código da barbearia (null para AdminCentral)</param>
/// <param name="BarbeariaNome">Nome da barbearia (null para AdminCentral)</param>
public record AuthenticationOutput(
    string UserId,
    string Name,
    string Role,
    Guid? BarbeariaId,
    string? BarbeariaCode,
    string? BarbeariaNome
);