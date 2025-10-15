using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IBarbershopServiceRepository
{
    Task<BarbershopService?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<BarbershopService>> ListAsync(
        Guid barbeariaId,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task InsertAsync(BarbershopService service, CancellationToken cancellationToken = default);
    Task UpdateAsync(BarbershopService service, CancellationToken cancellationToken = default);
}
