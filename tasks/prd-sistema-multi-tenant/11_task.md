---
status: completed
parallelizable: false
blocked_by: ["9.0", "10.0"]
---

<task_context>
<domain>Configuração de Infraestrutura</domain>
<type>Implementação</type>
<scope>API Setup</scope>
<complexity>média</complexity>
<dependencies>Todos os componentes anteriores</dependencies>
<unblocks>"12.0", "13.0"</unblocks>
</task_context>

# Tarefa 11.0: Configurar API e Pipeline

## Visão Geral
Configurar o Program.cs com toda a infraestrutura da aplicação: Dependency Injection, middlewares, database context, logging, CORS, Swagger e pipeline de requisições.

<requirements>
- Configuração de Dependency Injection para todos os serviços
- Configuração de DbContext com Entity Framework Core
- Configuração de middlewares na ordem correta
- Configuração de CORS para desenvolvimento e produção
- Configuração de Swagger/OpenAPI
- Configuração de logging estruturado
- Configuração de health checks
- Variáveis de ambiente para diferentes ambientes
</requirements>

## Subtarefas
- [ ] 11.1 Configurar Dependency Injection (services)
- [ ] 11.2 Configurar DbContext e migrations
- [ ] 11.3 Configurar pipeline de middlewares
- [ ] 11.4 Configurar CORS
- [ ] 11.5 Configurar Swagger/OpenAPI
- [ ] 11.6 Configurar logging estruturado
- [ ] 11.7 Configurar health checks
- [ ] 11.8 Criar arquivos de configuração por ambiente
- [ ] 11.9 Testar configuração em diferentes ambientes

## Sequenciamento
- **Bloqueado por**: 9.0 (Middlewares), 10.0 (Controllers)
- **Desbloqueia**: 12.0 (Swagger), 13.0 (Testes)
- **Paralelizável**: Não (integra todos os componentes)

## Detalhes de Implementação

### Program.cs Completo

```csharp
using BarbApp.Api;
using BarbApp.Api.Middlewares;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Interfaces;
using BarbApp.Infrastructure.Data;
using BarbApp.Infrastructure.Repositories;
using BarbApp.Infrastructure.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddDbContext<AppDbContext>(options =>
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
builder.Services.AddScoped<AuthenticateAdminCentralUseCase>();
builder.Services.AddScoped<AuthenticateAdminBarbeariaUseCase>();
builder.Services.AddScoped<AuthenticateBarbeiroUseCase>();
builder.Services.AddScoped<AuthenticateClienteUseCase>();
builder.Services.AddScoped<ListBarbeirosBarbeariaUseCase>();
builder.Services.AddScoped<TrocarContextoBarbeiroUseCase>();

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
        Version = "v1",
        Description = "API para sistema de gerenciamento de barbearias multi-tenant",
        Contact = new OpenApiContact
        {
            Name = "BarbApp Team",
            Email = "support@barbapp.com"
        }
    });

    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// ══════════════════════════════════════════════════════════
// DATABASE MIGRATION (Development)
// ══════════════════════════════════════════════════════════
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    Log.Information("Database migration completed");
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
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=barbapp;Username=barbapp_user;Password=barbapp_password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-characters-long-for-security",
    "Issuer": "BarbApp",
    "Audience": "BarbApp-Users",
    "ExpirationMinutes": 480
  },
  "Cors": {
    "AllowedOrigins": "https://barbapp.com,https://www.barbapp.com"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=barbapp_dev;Username=barbapp_user;Password=barbapp_password"
  },
  "JwtSettings": {
    "Secret": "development-secret-key-at-least-32-chars-long",
    "Issuer": "BarbApp-Dev",
    "Audience": "BarbApp-Dev-Users",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

### .csproj - XML Documentation

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <!-- PackageReferences here -->
</Project>
```

## Critérios de Sucesso
- ✅ Todos os serviços registrados corretamente no DI
- ✅ DbContext configurado e migrations funcionando
- ✅ Pipeline de middlewares na ordem correta
- ✅ CORS configurado para dev e produção
- ✅ Swagger acessível e funcional
- ✅ Logging estruturado funcionando
- ✅ Health checks respondem corretamente
- ✅ Aplicação inicia sem erros
- ✅ Configurações diferentes por ambiente funcionam
- ✅ Documentação XML gerada para Swagger

## Tempo Estimado
**3 horas**

## Referências
- TechSpec: Seção "4.6 Fase 1.6: Configuração Global"
- PRD: Seção "Requisitos de Infraestrutura"
- ASP.NET Core Configuration Best Practices
