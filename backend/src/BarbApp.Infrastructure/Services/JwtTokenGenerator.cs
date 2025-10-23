// BarbApp.Infrastructure/Services/JwtTokenGenerator.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BarbApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BarbApp.Infrastructure.Middlewares;

namespace BarbApp.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly string _secret;

    public JwtTokenGenerator(JwtSettings jwtSettings, ISecretManager secretManager)
    {
        _jwtSettings = jwtSettings;
        _secret = secretManager.GetSecretAsync("JWT_SECRET").GetAwaiter().GetResult();
        _jwtSettings.Secret = _secret;
    }

    public JwtToken GenerateToken(string userId, string userType, string email, Guid? barbeariaId, string? barbeariaCode = null)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_secret))
        {
            KeyId = "test-key"
        };

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, userType),
            new(ClaimTypes.Email, email)
        };

        if (barbeariaId.HasValue)
        {
            claims.Add(new("barbeariaId", barbeariaId.Value.ToString()));
        }

        if (!string.IsNullOrEmpty(barbeariaCode))
        {
            claims.Add(new("barbeariaCode", barbeariaCode));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            token.ValidTo
        );
    }

    public TokenClaims? Validate(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_secret))
            {
                KeyId = "test-key"
            };

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var barbeariaIdStr = principal.FindFirst("barbeariaId")?.Value;
            var barbeariaCode = principal.FindFirst("barbeariaCode")?.Value;

            Guid? barbeariaId = null;
            if (!string.IsNullOrEmpty(barbeariaIdStr))
                barbeariaId = Guid.Parse(barbeariaIdStr);

            return new TokenClaims(userId!, role!, barbeariaId, barbeariaCode);
        }
        catch (Exception)
        {
            return null;
        }
    }
}