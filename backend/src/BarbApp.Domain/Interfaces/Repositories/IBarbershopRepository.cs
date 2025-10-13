
using BarbApp.Domain.Common;
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories
{
    public interface IBarbershopRepository
    {
        Task<Barbershop?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Barbershop?> GetByCodeAsync(string code, CancellationToken cancellationToken);
        Task<Barbershop?> GetByDocumentAsync(string document, CancellationToken cancellationToken);
        Task<PaginatedResult<Barbershop>> ListAsync(
            int page,
            int pageSize,
            string? searchTerm,
            bool? isActive,
            string? sortBy,
            CancellationToken cancellationToken);
        Task InsertAsync(Barbershop barbershop, CancellationToken cancellationToken);
        Task UpdateAsync(Barbershop barbershop, CancellationToken cancellationToken);
        Task DeleteAsync(Barbershop barbershop, CancellationToken cancellationToken);
    }
}
