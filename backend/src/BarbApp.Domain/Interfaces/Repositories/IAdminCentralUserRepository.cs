// BarbApp.Domain/Interfaces/Repositories/IAdminCentralUserRepository.cs
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IAdminCentralUserRepository
{
    Task<AdminCentralUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AdminCentralUser> AddAsync(AdminCentralUser user, CancellationToken cancellationToken = default);
}