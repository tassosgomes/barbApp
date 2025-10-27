using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class ServicosRepository : IServicosRepository
{
    private readonly BarbAppDbContext _context;

    public ServicosRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BarbershopService>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.BarbershopServices
            .Where(s => s.BarbeariaId == barbeariaId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<BarbershopService?> GetByIdAsync(Guid servicoId, CancellationToken cancellationToken = default)
    {
        return await _context.BarbershopServices
            .FirstOrDefaultAsync(s => s.Id == servicoId, cancellationToken);
    }

    public async Task<List<BarbershopService>> GetByIdsAsync(List<Guid> servicosIds, CancellationToken cancellationToken = default)
    {
        return await _context.BarbershopServices
            .Where(s => servicosIds.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }
}