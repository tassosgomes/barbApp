// BarbApp.Infrastructure/Persistence/Repositories/BarberRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class BarberRepository : IBarberRepository
{
    private readonly BarbAppDbContext _context;

    public BarberRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<Barber?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .Include(b => b.Barbearia)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Barber?> GetByEmailAsync(Guid barbeariaId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .IgnoreQueryFilters()
            .Include(b => b.Barbearia)
            .FirstOrDefaultAsync(b =>
                b.Email == email.ToLowerInvariant() &&
                b.BarbeariaId == barbeariaId, cancellationToken);
    }

    public async Task<Barber?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .IgnoreQueryFilters()
            .Include(b => b.Barbearia)
            .FirstOrDefaultAsync(b => b.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<List<Barber>> ListAsync(
        Guid barbeariaId,
        bool? isActive = null,
        string? searchName = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Barbers
            .Include(b => b.Barbearia)
            .Where(b => b.BarbeariaId == barbeariaId);

        if (isActive.HasValue)
            query = query.Where(b => b.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(searchName))
            query = query.Where(b => b.Name.Contains(searchName));

        return await query
            .OrderBy(b => b.Name)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid barbeariaId,
        bool? isActive = null,
        string? searchName = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Barbers
            .Where(b => b.BarbeariaId == barbeariaId);

        if (isActive.HasValue)
            query = query.Where(b => b.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(searchName))
            query = query.Where(b => b.Name.Contains(searchName));

        return await query.CountAsync(cancellationToken);
    }

    public async Task InsertAsync(Barber barber, CancellationToken cancellationToken = default)
    {
        await _context.Barbers.AddAsync(barber, cancellationToken);
    }

    public Task UpdateAsync(Barber barber, CancellationToken cancellationToken = default)
    {
        _context.Barbers.Update(barber);
        return Task.CompletedTask;
    }

    // Legacy methods (deprecated)
    [Obsolete("Use GetByEmailAsync instead")]
    public async Task<Barber?> GetByTelefoneAndBarbeariaIdAsync(string telefone, Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .IgnoreQueryFilters()
            .Include(b => b.Barbearia)
            .FirstOrDefaultAsync(b =>
                b.Phone == telefone &&
                b.BarbeariaId == barbeariaId, cancellationToken);
    }

    [Obsolete("Use ListAsync instead")]
    public async Task<IEnumerable<Barber>> GetByBarbeariaIdAsync(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .Include(b => b.Barbearia)
            .Where(b => b.BarbeariaId == barbeariaId)
            .ToListAsync(cancellationToken);
    }
}