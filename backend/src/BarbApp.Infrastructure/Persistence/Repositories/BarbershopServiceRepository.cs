using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class BarbershopServiceRepository : IBarbershopServiceRepository
{
    private readonly BarbAppDbContext _context;

    public BarbershopServiceRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<BarbershopService?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BarbershopServices
            .Include(s => s.Barbearia)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<BarbershopService>> ListAsync(
        Guid barbeariaId,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BarbershopServices
            .Include(s => s.Barbearia)
            .Where(s => s.BarbeariaId == barbeariaId);

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(BarbershopService service, CancellationToken cancellationToken = default)
    {
        await _context.BarbershopServices.AddAsync(service, cancellationToken);
    }

    public Task UpdateAsync(BarbershopService service, CancellationToken cancellationToken = default)
    {
        _context.BarbershopServices.Update(service);
        return Task.CompletedTask;
    }
}
