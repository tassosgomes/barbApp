using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Health check para verificar o funcionamento do IMemoryCache.
/// Realiza operações básicas de set/get para validar que o cache está operacional.
/// </summary>
public class MemoryCacheHealthCheck : IHealthCheck
{
    private readonly IMemoryCache _cache;
    private const string HealthCheckKey = "healthcheck_memory_cache_test";

    public MemoryCacheHealthCheck(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Testar operação básica de cache: set, get, remove
            var testValue = Guid.NewGuid().ToString();

            // Set
            _cache.Set(HealthCheckKey, testValue, TimeSpan.FromSeconds(5));

            // Get
            var retrieved = _cache.TryGetValue(HealthCheckKey, out string? value);

            if (!retrieved || value != testValue)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Falha ao recuperar valor do cache após inserção",
                    data: new Dictionary<string, object>
                    {
                        ["retrieved"] = retrieved,
                        ["expectedValue"] = testValue,
                        ["actualValue"] = value ?? "null"
                    }));
            }

            // Remove
            _cache.Remove(HealthCheckKey);

            return Task.FromResult(HealthCheckResult.Healthy(
                "Cache de memória está funcionando corretamente",
                data: new Dictionary<string, object>
                {
                    ["operationsChecked"] = new[] { "Set", "Get", "Remove" }
                }));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Erro ao verificar cache de memória",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["exceptionType"] = ex.GetType().Name,
                    ["message"] = ex.Message
                }));
        }
    }
}
