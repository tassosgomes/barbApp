using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de serviços de landing pages
/// </summary>
public class LandingPageServiceRepository : ILandingPageServiceRepository
{
    private readonly BarbAppDbContext _context;

    public LandingPageServiceRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LandingPageService>> GetByLandingPageIdAsync(
        Guid landingPageId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LandingPageServices
            .Include(lps => lps.Service)
            .Where(lps => lps.LandingPageConfigId == landingPageId)
            .OrderBy(lps => lps.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByLandingPageIdAsync(
        Guid landingPageId, 
        CancellationToken cancellationToken = default)
    {
        var services = await _context.LandingPageServices
            .Where(lps => lps.LandingPageConfigId == landingPageId)
            .ToListAsync(cancellationToken);

        _context.LandingPageServices.RemoveRange(services);
    }

    public async Task<bool> ExistsAsync(
        Guid landingPageId, 
        Guid serviceId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LandingPageServices
            .AnyAsync(
                lps => lps.LandingPageConfigId == landingPageId && lps.ServiceId == serviceId,
                cancellationToken);
    }

    public async Task InsertAsync(LandingPageService service, CancellationToken cancellationToken = default)
    {
        await _context.LandingPageServices.AddAsync(service, cancellationToken);
    }

    public async Task InsertRangeAsync(IEnumerable<LandingPageService> services, CancellationToken cancellationToken = default)
    {
        await _context.LandingPageServices.AddRangeAsync(services, cancellationToken);
    }

    public Task UpdateAsync(LandingPageService service, CancellationToken cancellationToken = default)
    {
        _context.LandingPageServices.Update(service);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(LandingPageService service, CancellationToken cancellationToken = default)
    {
        _context.LandingPageServices.Remove(service);
        return Task.CompletedTask;
    }
}
