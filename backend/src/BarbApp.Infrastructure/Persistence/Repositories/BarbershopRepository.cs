// BarbApp.Infrastructure/Persistence/Repositories/BarbershopRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class BarbershopRepository : IBarbershopRepository
{
    private readonly BarbAppDbContext _context;

    public BarbershopRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<Barbershop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var barbeariaCode = BarbApp.Domain.ValueObjects.BarbeariaCode.Create(code);
            return await _context.Barbershops
                .FirstOrDefaultAsync(b => b.Code == barbeariaCode, cancellationToken);
        }
        catch (BarbApp.Domain.Exceptions.InvalidBarbeariaCodeException)
        {
            return null;
        }
    }

    public async Task<Barbershop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Barbershops
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Barbershop> AddAsync(Barbershop barbershop, CancellationToken cancellationToken = default)
    {
        await _context.Barbershops.AddAsync(barbershop, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return barbershop;
    }
}