// BarbApp.Domain/Interfaces/Repositories/IClienteRepository.cs
using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> GetByTelefoneAsync(string telefone, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(string telefone, CancellationToken cancellationToken = default);
}