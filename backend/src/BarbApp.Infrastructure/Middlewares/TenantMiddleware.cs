// BarbApp.Infrastructure/Middlewares/TenantMiddleware.cs
using BarbApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BarbApp.Infrastructure.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(
        RequestDelegate next,
        ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext)
    {
        _logger.LogInformation("TenantMiddleware: Starting for path {Path}", context.Request.Path);

        try
        {
            // Extrai informações do usuário autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("TenantMiddleware: User is authenticated");

                var userId = GetClaimValue(context, ClaimTypes.NameIdentifier);
                var email = GetClaimValue(context, ClaimTypes.Email);
                var userType = GetClaimValue(context, ClaimTypes.Role);
                var barbeariaIdStr = GetClaimValue(context, "barbeariaId");

                _logger.LogInformation(
                    "TenantMiddleware: Claims - userId={UserId}, email={Email}, userType={UserType}, barbeariaIdStr={BarbeariaIdStr}",
                    userId,
                    email,
                    userType,
                    barbeariaIdStr
                );

                Guid? barbeariaId = null;
                if (!string.IsNullOrEmpty(barbeariaIdStr) &&
                    Guid.TryParse(barbeariaIdStr, out var parsedId))
                {
                    barbeariaId = parsedId;
                }

                if (!string.IsNullOrEmpty(userId) &&
                    Guid.TryParse(userId, out var parsedUserId))
                {
                    _logger.LogInformation(
                        "Setting tenant context: UserId={UserId}, Role={Role}, BarbeariaId={BarbeariaId}",
                        parsedUserId,
                        userType,
                        barbeariaId
                    );

                    tenantContext.SetContext(
                        parsedUserId.ToString(),
                        userType ?? string.Empty,
                        barbeariaId,
                        null // barbeariaCode not in current claims
                    );

                    _logger.LogInformation(
                        "Tenant context set: UserId={UserId}, UserType={UserType}, BarbeariaId={BarbeariaId}",
                        parsedUserId,
                        userType,
                        barbeariaId
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to set tenant context: userId='{UserId}', parsedUserId={ParsedUserId}, userType='{UserType}', barbeariaId='{BarbeariaId}'",
                        userId,
                        Guid.TryParse(userId, out var _),
                        userType,
                        barbeariaId
                    );
                }
            }
            else
            {
                _logger.LogInformation("TenantMiddleware: User is not authenticated");
            }

            await _next(context);
        }
        finally
        {
            // Limpa o contexto após a requisição
            tenantContext.Clear();
        }
    }

    private static string? GetClaimValue(HttpContext context, string claimType)
    {
        return context.User.Claims
            .FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}