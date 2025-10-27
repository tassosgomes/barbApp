using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class BarbeirosRepository : IBarbeirosRepository
{
    private readonly BarbAppDbContext _context;

    public BarbeirosRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Barber>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .Where(b => b.BarbeariaId == barbeariaId && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Barber?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<bool> EstaAtivoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .AnyAsync(b => b.Id == id && b.IsActive, cancellationToken);
    }
}