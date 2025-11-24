using BarbApp.Application;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Cache em memória para disponibilidade de barbeiros.
/// Implementa métricas de cache hit rate para observabilidade.
/// </summary>
public class DisponibilidadeCache : IDisponibilidadeCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<DisponibilidadeCache> _logger;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);
    
    // Contadores para cálculo de hit rate
    private long _totalRequests = 0;
    private long _cacheHits = 0;

    public DisponibilidadeCache(IMemoryCache cache, ILogger<DisponibilidadeCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<DisponibilidadeOutput?> GetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        
        Interlocked.Increment(ref _totalRequests);
        
        var found = _cache.TryGetValue(key, out DisponibilidadeOutput? disponibilidade);
        
        if (found && disponibilidade != null)
        {
            Interlocked.Increment(ref _cacheHits);
            
            // Registrar métricas
            BarbAppMetrics.CacheOperationsCounter.WithLabels("disponibilidade", "hit").Inc();
            
            _logger.LogDebug(
                "Cache HIT para disponibilidade. BarbeiroId: {BarbeiroId}, DataInicio: {DataInicio}, DataFim: {DataFim}",
                barbeiroId, dataInicio, dataFim);
        }
        else
        {
            BarbAppMetrics.CacheOperationsCounter.WithLabels("disponibilidade", "miss").Inc();
            
            _logger.LogDebug(
                "Cache MISS para disponibilidade. BarbeiroId: {BarbeiroId}, DataInicio: {DataInicio}, DataFim: {DataFim}",
                barbeiroId, dataInicio, dataFim);
        }
        
        // Atualizar métrica de hit rate a cada 10 requisições
        if (_totalRequests % 10 == 0)
        {
            AtualizarHitRateMetrica();
        }
        
        return Task.FromResult(disponibilidade);
    }

    public Task SetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, DisponibilidadeOutput disponibilidade, CancellationToken cancellationToken = default)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        _cache.Set(key, disponibilidade, _ttl);
        
        BarbAppMetrics.CacheOperationsCounter.WithLabels("disponibilidade", "set").Inc();
        
        _logger.LogDebug(
            "Cache SET para disponibilidade. BarbeiroId: {BarbeiroId}, DataInicio: {DataInicio}, DataFim: {DataFim}, TTL: {TTL}",
            barbeiroId, dataInicio, dataFim, _ttl);
        
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        _cache.Remove(key);
        
        BarbAppMetrics.CacheOperationsCounter.WithLabels("disponibilidade", "invalidate").Inc();
        
        _logger.LogInformation(
            "Cache INVALIDADO para disponibilidade. BarbeiroId: {BarbeiroId}, DataInicio: {DataInicio}, DataFim: {DataFim}",
            barbeiroId, dataInicio, dataFim);
        
        return Task.CompletedTask;
    }

    private string GerarChave(Guid barbeiroId, DateTime dataInicio, DateTime dataFim)
    {
        return $"disponibilidade:{barbeiroId}:{dataInicio:yyyyMMdd}:{dataFim:yyyyMMdd}";
    }

    private void AtualizarHitRateMetrica()
    {
        var hitRate = _totalRequests > 0 
            ? (double)_cacheHits / _totalRequests 
            : 0.0;
        
        BarbAppMetrics.DisponibilidadeCacheHitRate.WithLabels("disponibilidade").Set(hitRate);
        
        _logger.LogDebug(
            "Cache hit rate atualizado. TotalRequests: {TotalRequests}, Hits: {Hits}, Rate: {Rate:P2}",
            _totalRequests, _cacheHits, hitRate);
    }
}