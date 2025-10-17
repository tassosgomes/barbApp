// BarbApp.Infrastructure/Persistence/Repositories/AdminBarbeariaUserRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class AdminBarbeariaUserRepository : IAdminBarbeariaUserRepository
{
    private readonly BarbAppDbContext _context;

    public AdminBarbeariaUserRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminBarbeariaUser?> GetByEmailAndBarbeariaIdAsync(string email, Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.AdminBarbeariaUsers
            .IgnoreQueryFilters()
            .Include(u => u.Barbearia)
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.BarbeariaId == barbeariaId, cancellationToken);
    }

    public async Task<AdminBarbeariaUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.AdminBarbeariaUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<AdminBarbeariaUser?> GetByBarbershopIdAsync(Guid barbershopId, CancellationToken cancellationToken = default)
    {
        return await _context.AdminBarbeariaUsers
            .IgnoreQueryFilters()
            .Include(u => u.Barbearia)
            .FirstOrDefaultAsync(u => u.BarbeariaId == barbershopId, cancellationToken);
    }

    public async Task<AdminBarbeariaUser> AddAsync(AdminBarbeariaUser user, CancellationToken cancellationToken = default)
    {
        await _context.AdminBarbeariaUsers.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(AdminBarbeariaUser user, CancellationToken cancellationToken = default)
    {
        _context.AdminBarbeariaUsers.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}