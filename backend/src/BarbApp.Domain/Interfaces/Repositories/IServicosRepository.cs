using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IServicosRepository
{
    Task<List<BarbershopService>> GetAtivosAsync(Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<BarbershopService?> GetByIdAsync(Guid servicoId, CancellationToken cancellationToken = default);
    Task<List<BarbershopService>> GetByIdsAsync(List<Guid> servicosIds, CancellationToken cancellationToken = default);
}