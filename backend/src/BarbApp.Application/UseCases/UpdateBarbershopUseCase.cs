using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class UpdateBarbershopUseCase : IUpdateBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBarbershopUseCase> _logger;

    public UpdateBarbershopUseCase(
        IBarbershopRepository barbershopRepository,
        IAddressRepository addressRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateBarbershopUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarbershopOutput> ExecuteAsync(UpdateBarbershopInput input, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting update of barbershop with ID {BarbershopId}", input.Id);

        var barbershop = await _barbershopRepository.GetByIdAsync(input.Id, cancellationToken);
        if (barbershop == null)
        {
            _logger.LogWarning("Barbershop with ID {BarbershopId} not found", input.Id);
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

        _logger.LogInformation("Barbershop with ID {BarbershopId} updated successfully", barbershop.Id);

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