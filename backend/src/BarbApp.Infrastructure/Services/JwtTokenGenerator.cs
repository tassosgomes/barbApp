// BarbApp.Infrastructure/Services/JwtTokenGenerator.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BarbApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BarbApp.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtToken GenerateToken(string userId, string userType, string email, Guid? barbeariaId, string? barbeariaCode = null)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

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
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
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
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
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