// BarbApp.Application/Interfaces/IJwtTokenGenerator.cs
namespace BarbApp.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(TokenClaims claims);
    TokenClaims? Validate(string token);
}

public record TokenClaims(
    string UserId,
    string Role,
    Guid? BarbeariaId,
    string? BarbeariaCode
);