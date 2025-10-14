// BarbApp.Infrastructure/Middlewares/SentryScopeEnrichmentMiddleware.cs
using System;
using BarbApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Sentry;
using Sentry.Protocol;
using System.Collections.Generic;
using System.Security.Claims;

namespace BarbApp.Infrastructure.Middlewares;

public class SentryScopeEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public SentryScopeEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("http.method", context.Request.Method);

            var route = context.GetEndpoint()?.DisplayName ?? context.Request.Path.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(route))
            {
                scope.SetTag("route", route);
            }

            if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
            {
                scope.SetTag("request_id", context.TraceIdentifier);
            }

            var tenantId = GetTenantId(context, tenantContext);
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                scope.SetTag("tenantId", tenantId);
            }

            var role = tenantContext.Role;
            if (!string.IsNullOrWhiteSpace(role))
            {
                scope.SetTag("role", role);
            }

            var userId = GetUserId(context, tenantContext);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                scope.SetTag("userId", userId);
                scope.User = new SentryUser
                {
                    Id = userId,
                    Email = context.User.FindFirstValue(ClaimTypes.Email),
                    Username = context.User.Identity?.Name,
                };
            }
        });

        await _next(context);
    }

    private static string? GetUserId(HttpContext context, ITenantContext tenantContext)
    {
        if (!string.IsNullOrWhiteSpace(tenantContext.UserId))
        {
            return tenantContext.UserId;
        }

        return context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private static string? GetTenantId(HttpContext context, ITenantContext tenantContext)
    {
        if (tenantContext.BarbeariaId.HasValue)
        {
            return tenantContext.BarbeariaId.Value.ToString();
        }

        var claimValue = context.User.FindFirstValue("barbeariaId");
        if (Guid.TryParse(claimValue, out var parsedId))
        {
            return parsedId.ToString();
        }

        return claimValue;
    }
}
