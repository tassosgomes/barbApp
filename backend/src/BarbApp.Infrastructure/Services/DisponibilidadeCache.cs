using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BarbApp.Infrastructure.Services;

public class DisponibilidadeCache : IDisponibilidadeCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

    public DisponibilidadeCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<DisponibilidadeOutput?> GetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        _cache.TryGetValue(key, out DisponibilidadeOutput? disponibilidade);
        return Task.FromResult(disponibilidade);
    }

    public Task SetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, DisponibilidadeOutput disponibilidade, CancellationToken cancellationToken = default)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        _cache.Set(key, disponibilidade, _ttl);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var key = GerarChave(barbeiroId, dataInicio, dataFim);
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    private string GerarChave(Guid barbeiroId, DateTime dataInicio, DateTime dataFim)
    {
        return $"disponibilidade:{barbeiroId}:{dataInicio:yyyyMMdd}:{dataFim:yyyyMMdd}";
    }
}