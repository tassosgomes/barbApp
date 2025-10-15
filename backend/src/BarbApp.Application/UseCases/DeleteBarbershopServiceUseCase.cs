using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class DeleteBarbershopServiceUseCase : IDeleteBarbershopServiceUseCase
{
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBarbershopServiceUseCase> _logger;

    public DeleteBarbershopServiceUseCase(
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<DeleteBarbershopServiceUseCase> logger)
    {
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid serviceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting deletion of service with ID {ServiceId}", serviceId);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Get existing service
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service == null || service.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning("Service with ID {ServiceId} not found or does not belong to barbearia {BarbeariaId}", serviceId, barbeariaId);
            throw new NotFoundException($"Service with ID {serviceId} not found");
        }

        // Soft delete by deactivating
        service.Deactivate();
        await _serviceRepository.UpdateAsync(service, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Service {ServiceId} deleted successfully (deactivated)", serviceId);
    }
}