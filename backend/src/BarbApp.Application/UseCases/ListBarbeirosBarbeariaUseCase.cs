// BarbApp.Application/UseCases/ListBarbeirosBarbeariaUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Domain.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class ListBarbeirosBarbeariaUseCase : IListBarbeirosBarbeariaUseCase
{
    private readonly IBarberRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ListBarbeirosBarbeariaUseCase(
        IBarberRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<BarberInfo>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var barbeariaId = _tenantContext.BarbeariaId;

        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        var barbers = await _repository.GetByBarbeariaIdAsync(barbeariaId.Value, cancellationToken);

        return barbers.Select(b => new BarberInfo
        {
            Id = b.Id,
            Nome = b.Name,
            Telefone = b.Telefone,
            BarbeariaId = b.BarbeariaId,
            NomeBarbearia = b.Barbearia.Name
        });
    }
}
