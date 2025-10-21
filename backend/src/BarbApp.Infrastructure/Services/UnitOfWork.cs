using BarbApp.Application.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly BarbAppDbContext _context;
    private ILandingPageConfigRepository? _landingPageConfigs;
    private ILandingPageServiceRepository? _landingPageServices;

    public UnitOfWork(BarbAppDbContext context)
    {
        _context = context;
    }

    public ILandingPageConfigRepository LandingPageConfigs
    {
        get
        {
            _landingPageConfigs ??= new LandingPageConfigRepository(_context);
            return _landingPageConfigs;
        }
    }

    public ILandingPageServiceRepository LandingPageServices
    {
        get
        {
            _landingPageServices ??= new LandingPageServiceRepository(_context);
            return _landingPageServices;
        }
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