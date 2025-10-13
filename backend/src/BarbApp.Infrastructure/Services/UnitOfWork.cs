using BarbApp.Application.Interfaces;
using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly BarbAppDbContext _context;

    public UnitOfWork(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task Commit(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task Rollback(CancellationToken cancellationToken)
    {
        // EF Core doesn't have explicit rollback, but we can detach entities
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }
    }
}