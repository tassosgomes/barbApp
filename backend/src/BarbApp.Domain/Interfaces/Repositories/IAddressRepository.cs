
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories
{
    public interface IAddressRepository
    {
        Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Address> AddAsync(Address address, CancellationToken cancellationToken = default);
    }
}
