
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly BarbAppDbContext _context;

        public AddressRepository(BarbAppDbContext context)
        {
            _context = context;
        }

        public async Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Addresses.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(Address address, CancellationToken cancellationToken = default)
        {
            await _context.Addresses.AddAsync(address, cancellationToken);
        }
    }
}
