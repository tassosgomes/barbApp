using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BarbApp.Infrastructure.Middlewares;

/// <summary>
/// Middleware para gerenciar Correlation ID em todas as requisições.
/// O Correlation ID permite rastrear uma requisição através de todo o sistema.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    public const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Tentar obter Correlation ID do header da requisição, ou gerar um novo
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                          ?? Guid.NewGuid().ToString("N");

        // Armazenar no HttpContext para uso em outros lugares
        context.Items[CorrelationIdHeader] = correlationId;
        
        // Adicionar ao header de resposta para rastreamento
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Enriquecer logs com Correlation ID usando Serilog LogContext
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            _logger.LogDebug("Request started with CorrelationId: {CorrelationId}", correlationId);
            
            await _next(context);
            
            _logger.LogDebug(
                "Request completed with CorrelationId: {CorrelationId}, StatusCode: {StatusCode}",
                correlationId,
                context.Response.StatusCode);
        }
    }
}

/// <summary>
/// Extension methods para configuração do Correlation ID Middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
