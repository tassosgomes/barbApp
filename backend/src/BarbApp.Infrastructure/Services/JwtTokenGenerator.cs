// BarbApp.Infrastructure/Services/JwtTokenGenerator.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BarbApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using BarbApp.Infrastructure.Middlewares;

namespace BarbApp.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly string _secret;
    private readonly ILogger<JwtTokenGenerator> _logger;

    public JwtTokenGenerator(JwtSettings jwtSettings, ISecretManager secretManager, ILogger<JwtTokenGenerator> logger)
    {
        _jwtSettings = jwtSettings;
        _logger = logger;
        try
        {
            _secret = secretManager.GetSecretAsync("JWT_SECRET").GetAwaiter().GetResult();
            _jwtSettings.Secret = _secret;
            _logger.LogInformation("JWT Secret loaded successfully from Infisical");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load JWT Secret from Infisical");
            // Fallback to configuration if available
            if (!string.IsNullOrEmpty(_jwtSettings.Secret))
            {
                _secret = _jwtSettings.Secret;
                _logger.LogInformation("Using JWT Secret from configuration as fallback");
            }
            else
            {
                throw new InvalidOperationException("JWT Secret not available from Infisical or configuration", ex);
            }
        }
    }

    public JwtToken GenerateToken(string userId, string userType, string email, Guid? barbeariaId, string? barbeariaCode = null)
    {
        // Validar tamanho mínimo da chave (256 bits = 32 bytes para HS256)
        if (_secret.Length < 32)
        {
            _logger.LogWarning("JWT Secret is shorter than recommended 32 characters. Current length: {Length}", _secret.Length);
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_secret));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, userType),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token ID único
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Issued At
            new(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Not Before
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
            notBefore: now,
            expires: now.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogDebug("JWT Token generated for user {UserId} with type {UserType}. Expires at: {ExpiresAt}", 
            userId, userType, token.ValidTo);

        return new JwtToken(tokenString, token.ValidTo);
    }

    public TokenClaims? Validate(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_secret));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5), // Tolerância de 5 minutos para diferenças de relógio
                RequireExpirationTime = true,
                RequireSignedTokens = true // IMPORTANTE: Requer que o token seja assinado
            };

            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            // Validação adicional: Verificar se o algoritmo é HS256
            if (validatedToken is JwtSecurityToken jwtToken)
            {
                if (jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                {
                    _logger.LogWarning("Token with invalid algorithm received: {Algorithm}", jwtToken.Header.Alg);
                    return null;
                }
            }

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
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Token expired: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning("Invalid token signature: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }
}