using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IBarbeirosRepository
{
    Task<List<Barber>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<Barber?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EstaAtivoAsync(Guid id, CancellationToken cancellationToken = default);
}