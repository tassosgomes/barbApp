// BarbApp.Infrastructure/Middlewares/GlobalExceptionHandlerMiddleware.cs
using BarbApp.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sentry;
using System.Net;
using System.Text.Json;

namespace BarbApp.Infrastructure.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> _logger)
    {
        _next = next;
        this._logger = _logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        SentrySdk.CaptureException(exception);

        _logger.LogError(
            exception,
            "An unhandled exception occurred: {Message}",
            exception.Message
        );

        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message),
            Domain.Exceptions.UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            Domain.Exceptions.InvalidUniqueCodeException => (HttpStatusCode.Unauthorized, "Código da barbearia inválido"),
            ForbiddenException => (HttpStatusCode.Forbidden, exception.Message),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message),
            DuplicateDocumentException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            DuplicateBarberException => (HttpStatusCode.Conflict, exception.Message),
            InvalidDocumentException => (HttpStatusCode.BadRequest, exception.Message),
            BarbeariaInactiveException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            FluentValidation.ValidationException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An error occurred processing your request")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

public record ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
