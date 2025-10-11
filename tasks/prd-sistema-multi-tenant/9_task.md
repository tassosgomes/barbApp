---
status: pending
parallelizable: false
blocked_by: ["8.0"]
---

<task_context>
<domain>Infraestrutura de API</domain>
<type>Implementação</type>
<scope>Middlewares</scope>
<complexity>média</complexity>
<dependencies>ASP.NET Core, JWT, TenantContext</dependencies>
<unblocks>"11.0"</unblocks>
</task_context>

# Tarefa 9.0: Implementar Middlewares de Autenticação e Tenant

## Visão Geral
Implementar os middlewares essenciais para processar requisições HTTP: TenantMiddleware para extrair e configurar contexto de tenant, configuração do pipeline de autenticação JWT e tratamento global de exceções.

<requirements>
- TenantMiddleware para extração de claims e configuração de contexto
- Configuração de autenticação JWT no pipeline ASP.NET Core
- Middleware de tratamento global de exceções
- Ordem correta de middlewares no pipeline
- Logging apropriado de operações de tenant
- Tratamento de erros 401/403
</requirements>

## Subtarefas
- [ ] 9.1 Implementar TenantMiddleware
- [ ] 9.2 Configurar autenticação JWT no pipeline
- [ ] 9.3 Implementar GlobalExceptionHandlerMiddleware
- [ ] 9.4 Configurar ordem de middlewares
- [ ] 9.5 Adicionar logging de operações
- [ ] 9.6 Criar testes de integração para middlewares

## Sequenciamento
- **Bloqueado por**: 8.0 (JWT e TenantContext)
- **Desbloqueia**: 11.0 (Configurar API e Pipeline)
- **Paralelizável**: Não (depende de serviços de segurança)

## Detalhes de Implementação

### TenantMiddleware

```csharp
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
                    tenantContext.SetCurrentTenant(
                        barbeariaId,
                        userType ?? string.Empty,
                        parsedUserId,
                        email ?? string.Empty
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
```

### GlobalExceptionHandlerMiddleware

```csharp
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        _logger.LogError(
            exception,
            "An unhandled exception occurred: {Message}",
            exception.Message
        );

        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An error occurred processing your request")
        };

        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
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
```

### Exceções Customizadas

```csharp
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

### Configuração do Pipeline de Autenticação

```csharp
// No Program.cs ou Startup.cs
public static class AuthenticationConfiguration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var result = JsonSerializer.Serialize(new ErrorResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Token de autenticação inválido ou ausente",
                        Timestamp = DateTime.UtcNow
                    });

                    return context.Response.WriteAsync(result);
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
```

### Extension Methods para Middlewares

```csharp
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
```

## Critérios de Sucesso
- ✅ TenantMiddleware extrai claims corretamente do token JWT
- ✅ TenantContext configurado apropriadamente para cada requisição
- ✅ TenantContext limpo após cada requisição
- ✅ Autenticação JWT funciona corretamente (401 para tokens inválidos)
- ✅ GlobalExceptionHandler trata exceções e retorna status codes apropriados
- ✅ Logging adequado de operações de tenant
- ✅ Ordem de middlewares está correta no pipeline
- ✅ Testes de integração cobrem cenários principais
- ✅ Tratamento de token expirado funciona
- ✅ Mensagens de erro são claras e consistentes

## Tempo Estimado
**3 horas**

## Referências
- TechSpec: Seção "4.4 Fase 1.4: JWT, Middlewares e Context"
- PRD: Seção "Requisitos de Segurança e Multi-tenant"
- ASP.NET Core Middleware Documentation
