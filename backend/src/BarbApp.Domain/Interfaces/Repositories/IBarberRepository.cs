using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IBarberRepository
{
    Task<Barber?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Barber?> GetByEmailAsync(Guid barbeariaId, string email, CancellationToken cancellationToken = default);
    Task<List<Barber>> ListAsync(
        Guid barbeariaId,
        bool? isActive = null,
        string? searchName = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        Guid barbeariaId,
        bool? isActive = null,
        string? searchName = null,
        CancellationToken cancellationToken = default);
    Task InsertAsync(Barber barber, CancellationToken cancellationToken = default);
    Task UpdateAsync(Barber barber, CancellationToken cancellationToken = default);
    
    // Legacy methods (to be removed in future tasks)
    [Obsolete("Use GetByEmailAsync instead. This method is deprecated and will be removed.")]
    Task<Barber?> GetByTelefoneAndBarbeariaIdAsync(string telefone, Guid barbeariaId, CancellationToken cancellationToken = default);
    
    [Obsolete("Use ListAsync instead. This method is deprecated and will be removed.")]
    Task<IEnumerable<Barber>> GetByBarbeariaIdAsync(Guid barbeariaId, CancellationToken cancellationToken = default);
}