// BarbApp.Domain/Interfaces/Repositories/ICustomerRepository.cs
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByTelefoneAndBarbeariaIdAsync(string telefone, Guid barbeariaId, CancellationToken cancellationToken = default);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);
}