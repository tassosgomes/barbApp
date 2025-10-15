using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class GetBarbershopServiceByIdUseCase : IGetBarbershopServiceByIdUseCase
{
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetBarbershopServiceByIdUseCase> _logger;

    public GetBarbershopServiceByIdUseCase(
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        ILogger<GetBarbershopServiceByIdUseCase> logger)
    {
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<BarbershopServiceOutput> ExecuteAsync(Guid serviceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting barbershop service by ID {ServiceId}", serviceId);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Get service
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service == null || service.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning("Service with ID {ServiceId} not found or does not belong to barbearia {BarbeariaId}", serviceId, barbeariaId);
            throw new NotFoundException($"Serviço com ID {serviceId} não encontrado");
        }

        _logger.LogInformation("Service {ServiceId} retrieved successfully", serviceId);

        return new BarbershopServiceOutput(
            service.Id,
            service.Name,
            service.Description,
            service.DurationMinutes,
            service.Price,
            service.IsActive);
    }
}