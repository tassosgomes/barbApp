using BarbApp.API;
using BarbApp.API.Extensions;
using BarbApp.API.Filters;
using BarbApp.Application.UseCases;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Infrastructure.Middlewares;
using BarbApp.Infrastructure.Services;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════
// LOGGING CONFIGURATION
// ══════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/barbapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ══════════════════════════════════════════════════════════
// DATABASE CONFIGURATION
// ══════════════════════════════════════════════════════════
builder.Services.AddDbContext<BarbAppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("BarbApp.Infrastructure")
    );

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ══════════════════════════════════════════════════════════
// DEPENDENCY INJECTION - Infrastructure
// ══════════════════════════════════════════════════════════
builder.Services.AddScoped<IAdminCentralUserRepository, AdminCentralUserRepository>();
builder.Services.AddScoped<IAdminBarbeariaUserRepository, AdminBarbeariaUserRepository>();
builder.Services.AddScoped<IBarberRepository, BarberRepository>();
builder.Services.AddScoped<IBarbershopRepository, BarbershopRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Security Services
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// ══════════════════════════════════════════════════════════
// DEPENDENCY INJECTION - Use Cases
// ══════════════════════════════════════════════════════════
builder.Services.AddScoped<IAuthenticateAdminCentralUseCase, AuthenticateAdminCentralUseCase>();
builder.Services.AddScoped<IAuthenticateAdminBarbeariaUseCase, AuthenticateAdminBarbeariaUseCase>();
builder.Services.AddScoped<IAuthenticateBarbeiroUseCase, AuthenticateBarbeiroUseCase>();
builder.Services.AddScoped<IAuthenticateClienteUseCase, AuthenticateClienteUseCase>();
builder.Services.AddScoped<IListBarbeirosBarbeariaUseCase, ListBarbeirosBarbeariaUseCase>();
builder.Services.AddScoped<ITrocarContextoBarbeiroUseCase, TrocarContextoBarbeiroUseCase>();

// ══════════════════════════════════════════════════════════
// AUTHENTICATION & AUTHORIZATION
// ══════════════════════════════════════════════════════════
builder.Services.AddJwtAuthentication(builder.Configuration);

// ══════════════════════════════════════════════════════════
// CONTROLLERS & VALIDATION
// ══════════════════════════════════════════════════════════
builder.Services.AddControllers();
builder.Services.AddFluentValidationConfiguration();

// ══════════════════════════════════════════════════════════
// CORS CONFIGURATION
// ══════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]!.Split(','))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ══════════════════════════════════════════════════════════
// SWAGGER/OPENAPI CONFIGURATION
// ══════════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BarbApp API",
        Version = "v1.0.0",
        Description = @"
            API REST para sistema de gerenciamento de barbearias multi-tenant.

            ## Autenticação
            Esta API utiliza JWT Bearer tokens para autenticação.

            ### Como obter um token:
            1. Faça login usando um dos endpoints de autenticação
            2. Copie o token retornado no campo 'token'
            3. Clique no botão 'Authorize' no topo desta página
            4. Digite 'Bearer {seu_token}' no campo de entrada
            5. Clique em 'Authorize' para salvar

            ### Tipos de Usuário:
            - **AdminCentral**: Acesso total ao sistema
            - **AdminBarbearia**: Acesso administrativo a uma barbearia específica
            - **Barbeiro**: Acesso a operações de barbeiro em uma ou mais barbearias
            - **Cliente**: Acesso a funcionalidades de cliente

            ## Multi-tenancy
            O sistema suporta múltiplas barbearias isoladas.
            Usuários AdminBarbearia e Barbeiro têm acesso restrito aos dados de suas barbearias.
        ",
        Contact = new OpenApiContact
        {
            Name = "BarbApp Team",
            Email = "support@barbapp.com",
            Url = new Uri("https://barbapp.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando Bearer scheme.
                      Digite 'Bearer' [espaço] e então seu token.
                      Exemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Custom schema IDs
    c.CustomSchemaIds(type => type.FullName);

    // Add examples
    c.SchemaFilter<SwaggerExamplesSchemaFilter>();
});

// ══════════════════════════════════════════════════════════
// HEALTH CHECKS
// ══════════════════════════════════════════════════════════
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        timeout: TimeSpan.FromSeconds(5));

// ══════════════════════════════════════════════════════════
// BUILD APPLICATION
// ══════════════════════════════════════════════════════════
var app = builder.Build();

// ══════════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// ══════════════════════════════════════════════════════════

// Global exception handling (first)
app.UseGlobalExceptionHandler();

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarbApp API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCors" : "ProductionCors");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Tenant middleware (after authentication)
app.UseTenantMiddleware();

// Controllers
app.MapControllers();

// Health checks
app.MapHealthChecks("/health");

// Test endpoints for middleware testing
app.MapGet("/test/unauthorized", () => { throw new BarbApp.Domain.Exceptions.UnauthorizedException("Test unauthorized"); });
app.MapGet("/test/forbidden", () => { throw new BarbApp.Domain.Exceptions.ForbiddenException("Test forbidden"); });
app.MapGet("/test/notfound", () => { throw new BarbApp.Domain.Exceptions.NotFoundException("Test not found"); });
app.MapGet("/test/validation", () => { throw new FluentValidation.ValidationException("Test validation"); });
app.MapGet("/test/unhandled", () => { throw new Exception("Test unhandled"); });
app.MapGet("/test/tenant-context", () => "Tenant context test");

// Weather forecast endpoint for authentication testing
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast")
.WithOpenApi();

// ══════════════════════════════════════════════════════════
// DATABASE MIGRATION (Development - Relational DB only)
// ══════════════════════════════════════════════════════════
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Only migrate if using a relational database provider
    if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migration completed");
    }
    else
    {
        logger.LogInformation("Using in-memory database - skipping migration");
    }
}

// ══════════════════════════════════════════════════════════
// RUN APPLICATION
// ══════════════════════════════════════════════════════════
try
{
    Log.Information("Starting BarbApp API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
