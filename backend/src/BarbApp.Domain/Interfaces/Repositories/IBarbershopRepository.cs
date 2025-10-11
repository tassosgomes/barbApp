// BarbApp.Domain/Interfaces/Repositories/IBarbershopRepository.cs
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IBarbershopRepository
{
    Task<Barbershop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Barbershop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Barbershop> AddAsync(Barbershop barbershop, CancellationToken cancellationToken = default);
}