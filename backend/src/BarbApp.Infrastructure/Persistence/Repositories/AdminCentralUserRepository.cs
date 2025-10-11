// BarbApp.Infrastructure/Persistence/Repositories/AdminCentralUserRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class AdminCentralUserRepository : IAdminCentralUserRepository
{
    private readonly BarbAppDbContext _context;

    public AdminCentralUserRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminCentralUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.AdminCentralUsers
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<AdminCentralUser> AddAsync(AdminCentralUser user, CancellationToken cancellationToken = default)
    {
        await _context.AdminCentralUsers.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }
}