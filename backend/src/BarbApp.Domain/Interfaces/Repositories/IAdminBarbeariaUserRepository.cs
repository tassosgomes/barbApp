// BarbApp.Domain/Interfaces/Repositories/IAdminBarbeariaUserRepository.cs
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IAdminBarbeariaUserRepository
{
    Task<AdminBarbeariaUser?> GetByEmailAndBarbeariaIdAsync(string email, Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<AdminBarbeariaUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AdminBarbeariaUser> AddAsync(AdminBarbeariaUser user, CancellationToken cancellationToken = default);
}