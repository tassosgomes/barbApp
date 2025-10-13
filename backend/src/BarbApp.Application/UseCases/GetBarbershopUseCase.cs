using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class GetBarbershopUseCase : IGetBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly ILogger<GetBarbershopUseCase> _logger;

    public GetBarbershopUseCase(IBarbershopRepository barbershopRepository, ILogger<GetBarbershopUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _logger = logger;
    }

    public async Task<BarbershopOutput> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving barbershop with ID {BarbershopId}", id);

        var barbershop = await _barbershopRepository.GetByIdAsync(id, cancellationToken);
        if (barbershop == null)
        {
            _logger.LogWarning("Barbershop with ID {BarbershopId} not found", id);
            throw new BarbershopNotFoundException("Barbearia não encontrada");
        }

        _logger.LogInformation("Barbershop with ID {BarbershopId} retrieved successfully", id);

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