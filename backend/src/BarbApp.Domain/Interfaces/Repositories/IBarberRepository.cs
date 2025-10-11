// BarbApp.Domain/Interfaces/Repositories/IBarberRepository.cs
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IBarberRepository
{
    Task<Barber?> GetByTelefoneAndBarbeariaIdAsync(string telefone, Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Barber>> GetByBarbeariaIdAsync(Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<Barber> AddAsync(Barber barber, CancellationToken cancellationToken = default);
}