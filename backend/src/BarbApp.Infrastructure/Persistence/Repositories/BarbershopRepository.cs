
using BarbApp.Domain.Common;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class BarbershopRepository : IBarbershopRepository
{
    private readonly BarbAppDbContext _context;

    public BarbershopRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<Barbershop?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Barbershops.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Barbershop?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.Barbershops.FirstOrDefaultAsync(b => b.Code.Value == code, cancellationToken);
    }

    public async Task<Barbershop?> GetByDocumentAsync(string document, CancellationToken cancellationToken)
    {
        return await _context.Barbershops.FirstOrDefaultAsync(b => b.Document.Value == document, cancellationToken);
    }

    public async Task<PaginatedResult<Barbershop>> ListAsync(
        int page, int pageSize, string? searchTerm, bool? isActive, string? sortBy, CancellationToken cancellationToken)
    {
        var query = _context.Barbershops.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(b => 
                b.Name.Contains(searchTerm) || 
                b.Code.Value.Contains(searchTerm) || 
                b.Document.Value.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortBy.ToLowerInvariant() switch
            {
                "name" => query.OrderBy(b => b.Name),
                "createdat" => query.OrderByDescending(b => b.CreatedAt),
                _ => query.OrderBy(b => b.Name)
            };
        }
        else
        {
            query = query.OrderBy(b => b.Name);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedResult<Barbershop>(items, totalCount, page, pageSize);
    }

    public async Task InsertAsync(Barbershop barbershop, CancellationToken cancellationToken)
    {
        await _context.Barbershops.AddAsync(barbershop, cancellationToken);
    }

    public Task UpdateAsync(Barbershop barbershop, CancellationToken cancellationToken)
    {
        _context.Barbershops.Update(barbershop);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Barbershop barbershop, CancellationToken cancellationToken)
    {
        _context.Barbershops.Remove(barbershop);
        return Task.CompletedTask;
    }
}