// BarbApp.Application/Interfaces/IJwtTokenGenerator.cs
namespace BarbApp.Application.Interfaces;

public interface IJwtTokenGenerator
{
    JwtToken GenerateToken(string userId, string userType, string email, Guid? barbeariaId, string? barbeariaCode = null);
    TokenClaims? Validate(string token);
}

public record TokenClaims(
    string UserId,
    string Role,
    Guid? BarbeariaId,
    string? BarbeariaCode
);

public record JwtToken(
    string Value,
    DateTime ExpiresAt
);