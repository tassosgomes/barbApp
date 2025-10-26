// BarbApp.Infrastructure/Persistence/Repositories/ClienteRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly BarbAppDbContext _context;

    public ClienteRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByTelefoneAsync(string telefone, CancellationToken cancellationToken = default)
    {
        // Global Query Filter já aplica filtro por barbeariaId
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Telefone == telefone, cancellationToken);
    }

    public async Task<Cliente?> GetByIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == clienteId, cancellationToken);
    }

    public async Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
    }

    public async Task<bool> ExisteAsync(string telefone, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .AnyAsync(c => c.Telefone == telefone, cancellationToken);
    }
}