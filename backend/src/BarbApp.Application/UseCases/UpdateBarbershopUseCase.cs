using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class UpdateBarbershopUseCase : IUpdateBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBarbershopUseCase(
        IBarbershopRepository barbershopRepository,
        IAddressRepository addressRepository,
        IUnitOfWork unitOfWork)
    {
        _barbershopRepository = barbershopRepository;
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BarbershopOutput> ExecuteAsync(UpdateBarbershopInput input, CancellationToken cancellationToken)
    {
        var barbershop = await _barbershopRepository.GetByIdAsync(input.Id, cancellationToken);
        if (barbershop == null)
        {
            throw new BarbershopNotFoundException("Barbearia não encontrada");
        }

        // Update address
        var address = Address.Create(
            input.ZipCode.Replace("-", ""),
            input.Street,
            input.Number,
            input.Complement,
            input.Neighborhood,
            input.City,
            input.State);

        barbershop.Address.Update(
            address.ZipCode,
            address.Street,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.State);

        // Update barbershop
        barbershop.Update(
            input.Name,
            input.Phone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", ""),
            input.OwnerName,
            input.Email,
            barbershop.Address,
            "AdminCentral"); // TODO: Get from context

        await _barbershopRepository.UpdateAsync(barbershop, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

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