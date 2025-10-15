using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class CreateBarbershopServiceUseCase : ICreateBarbershopServiceUseCase
{
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBarbershopServiceUseCase> _logger;

    public CreateBarbershopServiceUseCase(
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<CreateBarbershopServiceUseCase> logger)
    {
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarbershopServiceOutput> ExecuteAsync(CreateBarbershopServiceInput input, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting creation of new service with name {ServiceName}", input.Name);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Create service
        var service = BarbershopService.Create(
            barbeariaId.Value,
            input.Name,
            input.Description,
            input.DurationMinutes,
            input.Price);

        await _serviceRepository.InsertAsync(service, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Service created successfully with ID {ServiceId}", service.Id);

        return new BarbershopServiceOutput(
            service.Id,
            service.Name,
            service.Description,
            service.DurationMinutes,
            service.Price,
            service.IsActive);
    }
}