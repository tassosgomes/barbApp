using BarbApp.Domain.Exceptions;
using BarbApp.Infrastructure.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add other services (to be added later)
// builder.Services.AddScoped<ITenantContext, TenantContext>();

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
// app.UseTenantMiddleware(); // To be added after TenantContext is registered

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
