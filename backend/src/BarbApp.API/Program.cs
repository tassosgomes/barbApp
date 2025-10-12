using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Infrastructure.Middlewares;
using BarbApp.Infrastructure.Services;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using BarbApp.Application.UseCases;
using BarbApp.Application.Validators;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add controllers
builder.Services.AddControllers();

// Add authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add FluentValidation
// TODO: Configure FluentValidation properly
// var applicationAssembly = AppDomain.CurrentDomain.GetAssemblies()
//     .FirstOrDefault(a => a.GetName().Name == "BarbApp.Application");
// if (applicationAssembly != null)
// {
//     builder.Services.AddValidatorsFromAssembly(applicationAssembly);
// }
// builder.Services.AddFluentValidationAutoValidation();

// Add use cases
builder.Services.AddScoped<IAuthenticateAdminCentralUseCase, AuthenticateAdminCentralUseCase>();
builder.Services.AddScoped<IAuthenticateAdminBarbeariaUseCase, AuthenticateAdminBarbeariaUseCase>();
builder.Services.AddScoped<IAuthenticateBarbeiroUseCase, AuthenticateBarbeiroUseCase>();
builder.Services.AddScoped<IAuthenticateClienteUseCase, AuthenticateClienteUseCase>();
builder.Services.AddScoped<IListBarbeirosBarbeariaUseCase, ListBarbeirosBarbeariaUseCase>();
builder.Services.AddScoped<ITrocarContextoBarbeiroUseCase, TrocarContextoBarbeiroUseCase>();

// Add database context
builder.Services.AddDbContext<BarbAppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<IAdminCentralUserRepository, AdminCentralUserRepository>();
builder.Services.AddScoped<IAdminBarbeariaUserRepository, AdminBarbeariaUserRepository>();
builder.Services.AddScoped<IBarbershopRepository, BarbershopRepository>();
builder.Services.AddScoped<IBarberRepository, BarberRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Add tenant context
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Add infrastructure services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add middlewares in correct order
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantMiddleware();

// Map controllers
app.MapControllers();

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

// Test endpoints for middleware testing
app.MapGet("/test/unauthorized", () => { throw new UnauthorizedException("Test unauthorized"); });
app.MapGet("/test/forbidden", () => { throw new ForbiddenException("Test forbidden"); });
app.MapGet("/test/notfound", () => { throw new NotFoundException("Test not found"); });
app.MapGet("/test/validation", () => { throw new ValidationException("Test validation"); });
app.MapGet("/test/unhandled", () => { throw new Exception("Test unhandled"); });
app.MapGet("/test/tenant-context", () => "Tenant context test");

app.Run();

public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
