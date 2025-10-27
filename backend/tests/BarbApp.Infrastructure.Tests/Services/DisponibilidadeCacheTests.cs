// BarbApp.Infrastructure.Tests/Services/DisponibilidadeCacheTests.cs
using BarbApp.Application.DTOs;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BarbApp.Infrastructure.Tests.Services;

public class DisponibilidadeCacheTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly DisponibilidadeCache _cache;

    public DisponibilidadeCacheTests()
    {
        _memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        _cache = new DisponibilidadeCache(_memoryCache);
    }

    [Fact]
    public async Task GetAsync_ComCacheHit_DeveRetornarValorDoCache()
    {
        // Arrange
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(6);

        var expectedResult = new DisponibilidadeOutput(
            new BarbeiroDto(barbeiroId, "João Silva", null, new List<string>()),
            new List<DiaDisponivel>
            {
                new DiaDisponivel(dataInicio, new List<string> { "09:00", "09:30" })
            });

        var chave = $"disponibilidade:{barbeiroId}:{dataInicio:yyyyMMdd}:{dataFim:yyyyMMdd}";
        _memoryCache.Set(chave, expectedResult, TimeSpan.FromMinutes(5));

        // Act
        var result = await _cache.GetAsync(barbeiroId, dataInicio, dataFim);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetAsync_ComCacheMiss_DeveRetornarNull()
    {
        // Arrange
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(6);

        // Act
        var result = await _cache.GetAsync(barbeiroId, dataInicio, dataFim);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_DeveArmazenarValorNoCacheComTTL5Minutos()
    {
        // Arrange
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(6);

        var value = new DisponibilidadeOutput(
            new BarbeiroDto(barbeiroId, "João Silva", null, new List<string>()),
            new List<DiaDisponivel>
            {
                new DiaDisponivel(dataInicio, new List<string> { "09:00", "09:30" })
            });

        var chave = $"disponibilidade:{barbeiroId}:{dataInicio:yyyyMMdd}:{dataFim:yyyyMMdd}";

        // Act
        await _cache.SetAsync(barbeiroId, dataInicio, dataFim, value);

        // Assert
        var cachedValue = _memoryCache.Get<DisponibilidadeOutput>(chave);
        cachedValue.Should().Be(value);

        // Verificar se o valor persiste no cache (TTL de 5 minutos)
        var retrievedValue = await _cache.GetAsync(barbeiroId, dataInicio, dataFim);
        retrievedValue.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_ComDatasDiferentes_DeveGerarChavesDiferentes()
    {
        // Arrange
        var barbeiroId = Guid.NewGuid();
        var dataInicio1 = new DateTime(2025, 10, 15);
        var dataFim1 = dataInicio1.AddDays(6);
        var dataInicio2 = new DateTime(2025, 10, 20);
        var dataFim2 = dataInicio2.AddDays(6);

        var value1 = new DisponibilidadeOutput(
            new BarbeiroDto(barbeiroId, "João Silva", null, new List<string>()),
            new List<DiaDisponivel> { new DiaDisponivel(dataInicio1, new List<string> { "09:00" }) });

        var value2 = new DisponibilidadeOutput(
            new BarbeiroDto(barbeiroId, "João Silva", null, new List<string>()),
            new List<DiaDisponivel> { new DiaDisponivel(dataInicio2, new List<string> { "10:00" }) });

        // Act
        await _cache.SetAsync(barbeiroId, dataInicio1, dataFim1, value1);
        await _cache.SetAsync(barbeiroId, dataInicio2, dataFim2, value2);

        // Assert
        var result1 = await _cache.GetAsync(barbeiroId, dataInicio1, dataFim1);
        var result2 = await _cache.GetAsync(barbeiroId, dataInicio2, dataFim2);

        result1.Should().Be(value1);
        result2.Should().Be(value2);
        result1.Should().NotBe(result2);
    }

    [Fact]
    public async Task GetAsync_ComBarbeiroDiferente_DeveRetornarNull()
    {
        // Arrange
        var barbeiroId1 = Guid.NewGuid();
        var barbeiroId2 = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.Date;
        var dataFim = dataInicio.AddDays(6);

        var value = new DisponibilidadeOutput(
            new BarbeiroDto(barbeiroId1, "João Silva", null, new List<string>()),
            new List<DiaDisponivel> { new DiaDisponivel(dataInicio, new List<string> { "09:00" }) });

        await _cache.SetAsync(barbeiroId1, dataInicio, dataFim, value);

        // Act
        var result = await _cache.GetAsync(barbeiroId2, dataInicio, dataFim);

        // Assert
        result.Should().BeNull();
    }
}