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

    public async Task<Barber?> GetByTelefoneAndBarbeariaIdAsync(string telefone, Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .IgnoreQueryFilters()
            .Include(b => b.Barbearia)
            .FirstOrDefaultAsync(b =>
                b.Telefone == telefone &&
                b.BarbeariaId == barbeariaId, cancellationToken);
    }

    public async Task<IEnumerable<Barber>> GetByBarbeariaIdAsync(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.Barbers
            .Include(b => b.Barbearia)
            .Where(b => b.BarbeariaId == barbeariaId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Barber> AddAsync(Barber barber, CancellationToken cancellationToken = default)
    {
        await _context.Barbers.AddAsync(barber, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return barber;
    }
}