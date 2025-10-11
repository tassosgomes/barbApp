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
        try
        {
            // Extrai informações do usuário autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = GetClaimValue(context, ClaimTypes.NameIdentifier);
                var email = GetClaimValue(context, ClaimTypes.Email);
                var userType = GetClaimValue(context, "user_type");
                var barbeariaIdStr = GetClaimValue(context, "barbearia_id");

                Guid? barbeariaId = null;
                if (!string.IsNullOrEmpty(barbeariaIdStr) &&
                    Guid.TryParse(barbeariaIdStr, out var parsedId))
                {
                    barbeariaId = parsedId;
                }

                if (!string.IsNullOrEmpty(userId) &&
                    Guid.TryParse(userId, out var parsedUserId))
                {
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