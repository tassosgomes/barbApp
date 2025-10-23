using BarbApp.API;
using BarbApp.API.Extensions;
using BarbApp.API.Filters;
using BarbApp.Application.UseCases;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Application.Configuration;
using BarbApp.Infrastructure.Configuration;
using BarbApp.Infrastructure.Middlewares;
using BarbApp.Infrastructure.Options;
using BarbApp.Infrastructure.Services;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Sentry.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using DotNetEnv;

// ══════════════════════════════════════════════════════════
// LOAD ENVIRONMENT VARIABLES FROM .env FILE
// ══════════════════════════════════════════════════════════
// Carregar .env do diretório backend (não do diretório API)
var backendRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..");
var envPath = Path.Combine(backendRoot, ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"✓ Loaded .env from: {envPath}");
}
else
{
    Console.WriteLine($"⚠ .env file not found at: {envPath}");
    Console.WriteLine("  Environment variables will be loaded from system/container environment");
}

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════
// OVERRIDE CONFIGURATION WITH ENVIRONMENT VARIABLES
// ══════════════════════════════════════════════════════════
// Replace ${VAR} placeholders with actual environment values
var overrides = new Dictionary<string, string?>
{
    ["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
    ["JwtSettings:Secret"] = Environment.GetEnvironmentVariable("JWT_SECRET"),
    ["JwtSettings:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    ["JwtSettings:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
    ["AppSettings:FrontendUrl"] = Environment.GetEnvironmentVariable("FRONTEND_URL"),
    ["SmtpSettings:Host"] = Environment.GetEnvironmentVariable("SMTP_HOST"),
    ["SmtpSettings:Username"] = Environment.GetEnvironmentVariable("SMTP_USERNAME"),
    ["SmtpSettings:Password"] = Environment.GetEnvironmentVariable("SMTP_PASSWORD"),
    ["SmtpSettings:FromEmail"] = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL"),
    ["SmtpSettings:FromName"] = Environment.GetEnvironmentVariable("SMTP_FROM_NAME"),
    ["Sentry:Dsn"] = Environment.GetEnvironmentVariable("SENTRY_DSN"),
    ["Sentry:Environment"] = Environment.GetEnvironmentVariable("SENTRY_ENVIRONMENT"),
    ["Sentry:Release"] = Environment.GetEnvironmentVariable("SENTRY_RELEASE"),
    ["R2Storage:Endpoint"] = Environment.GetEnvironmentVariable("R2_ENDPOINT"),
    ["R2Storage:BucketName"] = Environment.GetEnvironmentVariable("R2_BUCKET_NAME"),
    ["R2Storage:PublicUrl"] = Environment.GetEnvironmentVariable("R2_PUBLIC_URL"),
    ["R2Storage:AccessKeyId"] = Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID"),
    ["R2Storage:SecretAccessKey"] = Environment.GetEnvironmentVariable("R2_SECRET_ACCESS_KEY")
};

// Only add non-null overrides
var nonNullOverrides = overrides
    .Where(kvp => kvp.Value != null)
    .Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value))
    .ToList();
    
if (nonNullOverrides.Any())
{
    builder.Configuration.AddInMemoryCollection(nonNullOverrides);
    Console.WriteLine($"✓ Applied {nonNullOverrides.Count} environment variable overrides");
}

// ══════════════════════════════════════════════════════════
// TESTING ENVIRONMENT DEFAULTS
// ══════════════════════════════════════════════════════════
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["JwtSettings:Secret"] = "test-secret-key-at-least-32-characters-long-for-jwt",
        ["JwtSettings:Issuer"] = "BarbApp-Test",
        ["JwtSettings:Audience"] = "BarbApp-Test-Users",
        ["JwtSettings:ExpirationMinutes"] = "60",
        ["Sentry:Dsn"] = string.Empty
    }!);
}

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
// SENTRY CONFIGURATION
// ══════════════════════════════════════════════════════════
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.WebHost.UseSentry(options =>
    {
        string? Get(string key, string envKey)
        {
            var v = builder.Configuration[key];
            var isPlaceholder = !string.IsNullOrWhiteSpace(v) && v.TrimStart().StartsWith("${") && v.TrimEnd().EndsWith("}");
            if (string.IsNullOrWhiteSpace(v) || isPlaceholder)
                v = Environment.GetEnvironmentVariable(envKey);
            return string.IsNullOrWhiteSpace(v) ? null : v;
        }

        options.Dsn = Get("Sentry:Dsn", "SENTRY_DSN");
        options.Environment = Get("Sentry:Environment", "SENTRY_ENVIRONMENT") ?? builder.Environment.EnvironmentName;
        options.Release = Get("Sentry:Release", "SENTRY_RELEASE");

        var tracesSampleRateStr = Get("Sentry:TracesSampleRate", "SENTRY_TRACES_SAMPLE_RATE");
        options.TracesSampleRate = double.TryParse(tracesSampleRateStr, out var rate) ? rate : 0.05;

        options.SendDefaultPii = false; // segurança por padrão
        options.IsGlobalModeEnabled = true; // captura fora do contexto de request
    });
}

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
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IBarbershopServiceRepository, BarbershopServiceRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Security Services
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IPasswordGenerator, SecurePasswordGenerator>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<ISecretManager, InfisicalService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IUniqueCodeGenerator, UniqueCodeGenerator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Email Service
// Usar FakeEmailService em desenvolvimento para não precisar de SMTP configurado
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
}

// JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// SMTP Settings
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// App Settings
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// R2 Storage Settings
builder.Services.Configure<R2StorageOptions>(
    builder.Configuration.GetSection("R2Storage"));

// ══════════════════════════════════════════════════════════
// DEPENDENCY INJECTION - Use Cases
// ══════════════════════════════════════════════════════════
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateBarbershopUseCase).Assembly);
});

builder.Services.AddScoped<IAuthenticateAdminCentralUseCase, AuthenticateAdminCentralUseCase>();
builder.Services.AddScoped<IAuthenticateAdminBarbeariaUseCase, AuthenticateAdminBarbeariaUseCase>();
builder.Services.AddScoped<IAuthenticateBarbeiroUseCase, AuthenticateBarbeiroUseCase>();
builder.Services.AddScoped<IAuthenticateClienteUseCase, AuthenticateClienteUseCase>();
builder.Services.AddScoped<IListBarbeirosBarbeariaUseCase, ListBarbeirosBarbeariaUseCase>();
builder.Services.AddScoped<ITrocarContextoBarbeiroUseCase, TrocarContextoBarbeiroUseCase>();
builder.Services.AddScoped<ICreateBarbershopUseCase, CreateBarbershopUseCase>();
builder.Services.AddScoped<IUpdateBarbershopUseCase, UpdateBarbershopUseCase>();
builder.Services.AddScoped<IDeleteBarbershopUseCase, DeleteBarbershopUseCase>();
builder.Services.AddScoped<IDeactivateBarbershopUseCase, DeactivateBarbershopUseCase>();
builder.Services.AddScoped<IReactivateBarbershopUseCase, ReactivateBarbershopUseCase>();
builder.Services.AddScoped<IGetBarbershopUseCase, GetBarbershopUseCase>();
builder.Services.AddScoped<IListBarbershopsUseCase, ListBarbershopsUseCase>();
builder.Services.AddScoped<IResendCredentialsUseCase, ResendCredentialsUseCase>();
builder.Services.AddScoped<IGetMyBarbershopUseCase, GetMyBarbershopUseCase>();
builder.Services.AddScoped<ValidateBarbeariaCodeUseCase>();
builder.Services.AddScoped<ICreateBarberUseCase, CreateBarberUseCase>();
builder.Services.AddScoped<IUpdateBarberUseCase, UpdateBarberUseCase>();
builder.Services.AddScoped<IRemoveBarberUseCase, RemoveBarberUseCase>();
builder.Services.AddScoped<IListBarbersUseCase, ListBarbersUseCase>();
builder.Services.AddScoped<IGetBarberByIdUseCase, GetBarberByIdUseCase>();
builder.Services.AddScoped<IResetBarberPasswordUseCase, ResetBarberPasswordUseCase>();
builder.Services.AddScoped<IGetTeamScheduleUseCase, GetTeamScheduleUseCase>();
builder.Services.AddScoped<ICreateBarbershopServiceUseCase, CreateBarbershopServiceUseCase>();
builder.Services.AddScoped<IUpdateBarbershopServiceUseCase, UpdateBarbershopServiceUseCase>();
builder.Services.AddScoped<IDeleteBarbershopServiceUseCase, DeleteBarbershopServiceUseCase>();
builder.Services.AddScoped<IListBarbershopServicesUseCase, ListBarbershopServicesUseCase>();
builder.Services.AddScoped<IGetBarbershopServiceByIdUseCase, GetBarbershopServiceByIdUseCase>();
builder.Services.AddScoped<IGetBarberScheduleUseCase, GetBarberScheduleUseCase>();
builder.Services.AddScoped<IGetAppointmentDetailsUseCase, GetAppointmentDetailsUseCase>();
builder.Services.AddScoped<IConfirmAppointmentUseCase, ConfirmAppointmentUseCase>();
builder.Services.AddScoped<ICancelAppointmentUseCase, CancelAppointmentUseCase>();
builder.Services.AddScoped<ICompleteAppointmentUseCase, CompleteAppointmentUseCase>();
builder.Services.AddScoped<ILandingPageService, LandingPageService>();
builder.Services.AddSingleton<IR2StorageService, R2StorageService>();
builder.Services.AddScoped<ILogoUploadService, R2LogoUploadService>();
builder.Services.AddScoped<IImageProcessor, ImageSharpProcessor>();

// ══════════════════════════════════════════════════════════
// OUTPUT CACHE CONFIGURATION
// ══════════════════════════════════════════════════════════
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
});

// ══════════════════════════════════════════════════════════
// RESPONSE COMPRESSION CONFIGURATION
// ══════════════════════════════════════════════════════════
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

// ══════════════════════════════════════════════════════════
// AUTHENTICATION & AUTHORIZATION
// ══════════════════════════════════════════════════════════
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Services.BuildServiceProvider());

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
                "http://localhost:3001",
                "http://localhost:5173",
                "https://barberapp.tasso.dev.br",
                "https://dev-barberapp.tasso.dev.br",
                "https://staging-barberapp.tasso.dev.br")
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

    options.AddPolicy("PublicLandingPage", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

    // Include XML comments from API project
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Include XML comments from Application project (DTOs)
    var applicationXmlFile = "BarbApp.Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        c.IncludeXmlComments(applicationXmlPath);
    }

    // Custom schema IDs
    c.CustomSchemaIds(type => type.FullName);

    // Add examples
    c.SchemaFilter<SwaggerExamplesSchemaFilter>();

    // Handle file uploads
    c.ParameterFilter<SwaggerFileUploadOperationFilter>();
    c.OperationFilter<SwaggerFileUploadOperationFilter>();
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

// CORS
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCors" : "ProductionCors");

// Response Compression
app.UseResponseCompression();

// Output Cache
app.UseOutputCache();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Prometheus metrics
app.UseHttpMetrics();
app.UseMetricServer();

// Tenant middleware (after authentication)
app.UseTenantMiddleware();
app.UseSentryScopeEnrichment();

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
