// BarbApp.Application/DTOs/AuthenticationOutput.cs
namespace BarbApp.Application.DTOs;

public record AuthenticationOutput(
    string UserId,
    string Name,
    string Role,
    Guid? BarbeariaId,
    string? BarbeariaCode,
    string? BarbeariaNome
);