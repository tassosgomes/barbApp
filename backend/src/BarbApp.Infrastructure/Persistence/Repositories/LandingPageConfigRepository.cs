using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de configurações de landing pages
/// </summary>
public class LandingPageConfigRepository : ILandingPageConfigRepository
{
    private readonly BarbAppDbContext _context;

    public LandingPageConfigRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<LandingPageConfig?> GetByBarbershopIdAsync(
        Guid barbershopId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LandingPageConfigs
            .Include(lp => lp.Barbershop)
            .Include(lp => lp.Services)
                .ThenInclude(lps => lps.Service)
            .FirstOrDefaultAsync(lp => lp.BarbershopId == barbershopId, cancellationToken);
    }

    public async Task<LandingPageConfig?> GetByBarbershopCodeAsync(
        string code, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LandingPageConfigs
            .Include(lp => lp.Barbershop)
            .FirstOrDefaultAsync(lp => lp.Barbershop.Code.Value == code, cancellationToken);
    }

    public async Task<LandingPageConfig?> GetByBarbershopIdWithServicesAsync(
        Guid barbershopId, 
        CancellationToken cancellationToken = default)
    {
        // Load the config with all services first
        var config = await _context.LandingPageConfigs
            .Include(lp => lp.Barbershop)
            .Include(lp => lp.Services)
                .ThenInclude(lps => lps.Service)
            .AsSplitQuery()
            .FirstOrDefaultAsync(lp => lp.BarbershopId == barbershopId, cancellationToken);

        return config;
    }

    public async Task<LandingPageConfig?> GetPublicByCodeAsync(
        string code, 
        CancellationToken cancellationToken = default)
    {
        // Load the config with all services, will be filtered by the application layer
        var config = await _context.LandingPageConfigs
            .Include(lp => lp.Barbershop)
            .Include(lp => lp.Services)
                .ThenInclude(lps => lps.Service)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                lp => lp.Barbershop.Code.Value == code && lp.IsPublished, 
                cancellationToken);

        return config;
    }

    public async Task<bool> ExistsForBarbershopAsync(
        Guid barbershopId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LandingPageConfigs
            .AnyAsync(lp => lp.BarbershopId == barbershopId, cancellationToken);
    }

    public async Task InsertAsync(LandingPageConfig config, CancellationToken cancellationToken = default)
    {
        await _context.LandingPageConfigs.AddAsync(config, cancellationToken);
    }

    public Task UpdateAsync(LandingPageConfig config, CancellationToken cancellationToken = default)
    {
        _context.LandingPageConfigs.Update(config);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(LandingPageConfig config, CancellationToken cancellationToken = default)
    {
        _context.LandingPageConfigs.Remove(config);
        return Task.CompletedTask;
    }
}
