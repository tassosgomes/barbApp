// BarbApp.Infrastructure/Persistence/Repositories/CustomerRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BarbAppDbContext _context;

    public CustomerRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByTelefoneAndBarbeariaIdAsync(string telefone, Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .IgnoreQueryFilters()
            .Include(c => c.Barbearia)
            .FirstOrDefaultAsync(c =>
                c.Telefone == telefone &&
                c.BarbeariaId == barbeariaId, cancellationToken);
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return customer;
    }
}