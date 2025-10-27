using BarbApp.API.Extensions;
using BarbApp.Application.UseCases;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Application.Configuration;
using BarbApp.Infrastructure.Configuration;
using BarbApp.Infrastructure.Options;
using BarbApp.Infrastructure.Services;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using BarbApp.Infrastructure.Middlewares;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Sentry.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using BarbApp.API.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BarbApp.API.Configuration;

public static class ServiceConfiguration
{
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BarbAppDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("BarbApp.Infrastructure")
            );

            // Enable sensitive data logging and detailed errors in development
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (environment == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
    }

    public static void ConfigureInfrastructureServices(this IServiceCollection services)
    {
        // Configuration options - MUST be configured before services that depend on them
        services.Configure<JwtSettings>(
            services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("JwtSettings"));
        services.Configure<SmtpSettings>(
            services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("SmtpSettings"));
        services.Configure<AppSettings>(
            services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("AppSettings"));
        services.Configure<R2StorageOptions>(
            services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("R2Storage"));

        // Register JwtSettings as a singleton service so it can be injected directly
        services.AddSingleton<JwtSettings>(sp =>
            sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        // Repository services
        services.AddScoped<IAdminCentralUserRepository, AdminCentralUserRepository>();
        services.AddScoped<IAdminBarbeariaUserRepository, AdminBarbeariaUserRepository>();
        services.AddScoped<IBarberRepository, BarberRepository>();
        services.AddScoped<IBarbeirosRepository, BarbeirosRepository>();
        services.AddScoped<IBarbershopRepository, BarbershopRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBarbershopServiceRepository, BarbershopServiceRepository>();
        services.AddScoped<IServicosRepository, ServicosRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();

        // Security services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IPasswordGenerator, SecurePasswordGenerator>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>(); // Now JwtSettings is available
        services.AddSingleton<ISecretManager, InfisicalService>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IUniqueCodeGenerator, UniqueCodeGenerator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDisponibilidadeCache, DisponibilidadeCache>();

        // Email service (conditional based on environment)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (environment == "Development")
        {
            services.AddScoped<IEmailService, FakeEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
    }

    public static void ConfigureUseCases(this IServiceCollection services)
    {
        // AutoMapper configuration
        services.AddAutoMapper(typeof(BarbApp.Application.Mappings.AutoMapperProfile).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateBarbershopUseCase).Assembly);
        });

        // Authentication use cases
        services.AddScoped<IAuthenticateAdminCentralUseCase, AuthenticateAdminCentralUseCase>();
        services.AddScoped<IAuthenticateAdminBarbeariaUseCase, AuthenticateAdminBarbeariaUseCase>();
        services.AddScoped<IAuthenticateBarbeiroUseCase, AuthenticateBarbeiroUseCase>();
        services.AddScoped<ICadastrarClienteUseCase, CadastrarClienteUseCase>();
        services.AddScoped<ILoginClienteUseCase, LoginClienteUseCase>();

        // Barbershop management
        services.AddScoped<IListBarbeirosBarbeariaUseCase, ListBarbeirosBarbeariaUseCase>();
        services.AddScoped<ITrocarContextoBarbeiroUseCase, TrocarContextoBarbeiroUseCase>();
        services.AddScoped<ICreateBarbershopUseCase, CreateBarbershopUseCase>();
        services.AddScoped<IUpdateBarbershopUseCase, UpdateBarbershopUseCase>();
        services.AddScoped<IDeleteBarbershopUseCase, DeleteBarbershopUseCase>();
        services.AddScoped<IDeactivateBarbershopUseCase, DeactivateBarbershopUseCase>();
        services.AddScoped<IReactivateBarbershopUseCase, ReactivateBarbershopUseCase>();
        services.AddScoped<IGetBarbershopUseCase, GetBarbershopUseCase>();
        services.AddScoped<IListBarbershopsUseCase, ListBarbershopsUseCase>();
        services.AddScoped<IResendCredentialsUseCase, ResendCredentialsUseCase>();
        services.AddScoped<IGetMyBarbershopUseCase, GetMyBarbershopUseCase>();
        services.AddScoped<ValidateBarbeariaCodeUseCase>();

        // Client listing use cases
        services.AddScoped<IListarBarbeirosUseCase, ListarBarbeirosUseCase>();
        services.AddScoped<IListarServicosUseCase, ListarServicosUseCase>();
        services.AddScoped<IConsultarDisponibilidadeUseCase, ConsultarDisponibilidadeUseCase>();

        // Barber management
        services.AddScoped<ICreateBarberUseCase, CreateBarberUseCase>();
        services.AddScoped<IUpdateBarberUseCase, UpdateBarberUseCase>();
        services.AddScoped<IRemoveBarberUseCase, RemoveBarberUseCase>();
        services.AddScoped<IListBarbersUseCase, ListBarbersUseCase>();
        services.AddScoped<IGetBarberByIdUseCase, GetBarberByIdUseCase>();
        services.AddScoped<IResetBarberPasswordUseCase, ResetBarberPasswordUseCase>();
        services.AddScoped<IGetTeamScheduleUseCase, GetTeamScheduleUseCase>();

        // Services management
        services.AddScoped<ICreateBarbershopServiceUseCase, CreateBarbershopServiceUseCase>();
        services.AddScoped<IUpdateBarbershopServiceUseCase, UpdateBarbershopServiceUseCase>();
        services.AddScoped<IDeleteBarbershopServiceUseCase, DeleteBarbershopServiceUseCase>();
        services.AddScoped<IListBarbershopServicesUseCase, ListBarbershopServicesUseCase>();
        services.AddScoped<IGetBarbershopServiceByIdUseCase, GetBarbershopServiceByIdUseCase>();

        // Appointments
        services.AddScoped<IGetBarberScheduleUseCase, GetBarberScheduleUseCase>();
        services.AddScoped<IGetAppointmentDetailsUseCase, GetAppointmentDetailsUseCase>();
        services.AddScoped<IConfirmAppointmentUseCase, ConfirmAppointmentUseCase>();
        services.AddScoped<ICancelAppointmentUseCase, CancelAppointmentUseCase>();
        services.AddScoped<ICompleteAppointmentUseCase, CompleteAppointmentUseCase>();

        // Other services
        services.AddScoped<ILandingPageService, LandingPageService>();
        services.AddSingleton<IR2StorageService, R2StorageService>();
        services.AddScoped<ILogoUploadService, R2LogoUploadService>();
        services.AddScoped<IImageProcessor, ImageSharpProcessor>();
    }

    public static void ConfigureFrameworkServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // Output Cache
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.Cache());
        });

        // Response Compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
        });

                // Authentication & Authorization
        // Build temporary service provider to get dependencies
        var tempServiceProvider = services.BuildServiceProvider();
        var jwtSettings = tempServiceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        var loggerFactory = tempServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("JwtAuthentication");
        
        // Get JWT secret from Infisical or fallback to configuration
        string jwtSecret;
        try
        {
            var secretManager = tempServiceProvider.GetRequiredService<ISecretManager>();
            jwtSecret = secretManager.GetSecretAsync("JWT_SECRET").GetAwaiter().GetResult();
            logger.LogInformation("JWT Secret loaded successfully from Infisical for authentication");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load JWT Secret from Infisical, falling back to configuration");
            jwtSecret = jwtSettings.Secret;
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret not available from Infisical or configuration", ex);
            }
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role // IMPORTANTE: Define qual claim contém as roles
            };

            // Adicionar eventos para debug
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    logger.LogWarning("JWT Authentication failed: {Message}", context.Exception.Message);
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    else if (context.Exception is SecurityTokenInvalidSignatureException)
                    {
                        logger.LogError("Invalid JWT signature detected");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>();
                    logger.LogDebug("JWT Token validated successfully. Claims: {Claims}", string.Join(", ", claims));
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    logger.LogWarning("JWT Challenge triggered. Error: {Error}, ErrorDescription: {ErrorDescription}", 
                        context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization();

        // Controllers & Validation
        services.AddControllers();
        services.AddFluentValidationConfiguration();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentCors", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:3000",
                        "http://localhost:3001",
                        "http://localhost:5173",
                        "https://barberapp.tasso.dev.br",
                        "https://dev-barberapp.tasso.dev.br",
                        "https://staging-barberapp.tasso.dev.br",
                        "https://dev-admbarberapp.tasso.dev.br",
                        "https://staging-admbarberapp.tasso.dev.br",
                        "https://admbarberapp.tasso.dev.br")
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

        // Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
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

        // Health checks
        services.AddHealthChecks()
            .AddNpgSql(
                builder.Configuration.GetConnectionString("DefaultConnection")!,
                name: "database",
                timeout: TimeSpan.FromSeconds(5));
    }
}