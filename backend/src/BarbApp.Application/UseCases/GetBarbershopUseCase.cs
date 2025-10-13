using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class GetBarbershopUseCase : IGetBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;

    public GetBarbershopUseCase(IBarbershopRepository barbershopRepository)
    {
        _barbershopRepository = barbershopRepository;
    }

    public async Task<BarbershopOutput> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var barbershop = await _barbershopRepository.GetByIdAsync(id, cancellationToken);
        if (barbershop == null)
        {
            throw new BarbershopNotFoundException("Barbearia não encontrada");
        }

        return new BarbershopOutput(
            barbershop.Id,
            barbershop.Name,
            barbershop.Document.Value,
            barbershop.Phone,
            barbershop.OwnerName,
            barbershop.Email,
            barbershop.Code.Value,
            barbershop.IsActive,
            new AddressOutput(
                barbershop.Address.ZipCode,
                barbershop.Address.Street,
                barbershop.Address.Number,
                barbershop.Address.Complement,
                barbershop.Address.Neighborhood,
                barbershop.Address.City,
                barbershop.Address.State),
            barbershop.CreatedAt,
            barbershop.UpdatedAt);
    }
}