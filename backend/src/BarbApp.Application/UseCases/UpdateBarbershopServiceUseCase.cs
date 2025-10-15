using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class UpdateBarbershopServiceUseCase : IUpdateBarbershopServiceUseCase
{
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBarbershopServiceUseCase> _logger;

    public UpdateBarbershopServiceUseCase(
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<UpdateBarbershopServiceUseCase> logger)
    {
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarbershopServiceOutput> ExecuteAsync(UpdateBarbershopServiceInput input, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting update of service with ID {ServiceId}", input.Id);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Get existing service
        var service = await _serviceRepository.GetByIdAsync(input.Id, cancellationToken);
        if (service == null || service.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning("Service with ID {ServiceId} not found or does not belong to barbearia {BarbeariaId}", input.Id, barbeariaId);
            throw new NotFoundException($"Service with ID {input.Id} not found");
        }

        // Update service
        service.Update(input.Name, input.Description, input.DurationMinutes, input.Price);

        await _serviceRepository.UpdateAsync(service, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Service updated successfully with ID {ServiceId}", service.Id);

        return new BarbershopServiceOutput(
            service.Id,
            service.Name,
            service.Description,
            service.DurationMinutes,
            service.Price,
            service.IsActive);
    }
}