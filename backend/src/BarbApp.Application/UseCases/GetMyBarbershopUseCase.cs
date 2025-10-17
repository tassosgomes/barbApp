using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class GetMyBarbershopUseCase : IGetMyBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly ITenantContext _tenantContext;

    public GetMyBarbershopUseCase(
        IBarbershopRepository barbershopRepository,
        ITenantContext tenantContext)
    {
        _barbershopRepository = barbershopRepository;
        _tenantContext = tenantContext;
    }

    public async Task<BarbershopOutput> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var barbeariaId = _tenantContext.BarbeariaId;

        if (barbeariaId == null)
            throw new ForbiddenException("Usuário não associado a uma barbearia");

        var barbearia = await _barbershopRepository.GetByIdAsync(barbeariaId.Value, cancellationToken);

        if (barbearia == null)
            throw new NotFoundException("Barbearia não encontrada");

        return new BarbershopOutput(
            Id: barbearia.Id,
            Name: barbearia.Name,
            Document: barbearia.Document.Value,
            Phone: barbearia.Phone,
            OwnerName: barbearia.OwnerName,
            Email: barbearia.Email,
            Code: barbearia.Code.Value,
            IsActive: barbearia.IsActive,
            Address: new AddressOutput(
                Street: barbearia.Address.Street,
                Number: barbearia.Address.Number,
                Complement: barbearia.Address.Complement,
                Neighborhood: barbearia.Address.Neighborhood,
                City: barbearia.Address.City,
                State: barbearia.Address.State,
                ZipCode: barbearia.Address.ZipCode
            ),
            CreatedAt: barbearia.CreatedAt,
            UpdatedAt: barbearia.UpdatedAt
        );
    }
}