using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListBarbershopServicesUseCase : IListBarbershopServicesUseCase
{
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListBarbershopServicesUseCase> _logger;

    public ListBarbershopServicesUseCase(
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        ILogger<ListBarbershopServicesUseCase> logger)
    {
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<PaginatedBarbershopServicesOutput> ExecuteAsync(
        bool? isActive,
        string? searchName,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing services with filters: isActive={IsActive}, searchName={SearchName}, page={Page}, pageSize={PageSize}",
            isActive, searchName, page, pageSize);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        // For now, since IBarbershopServiceRepository.ListAsync doesn't have search or pagination, load all and filter
        var allServices = await _serviceRepository.ListAsync(barbeariaId.Value, isActive, cancellationToken);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            allServices = allServices.Where(s => s.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var totalCount = allServices.Count;

        // Apply pagination
        var offset = (page - 1) * pageSize;
        var paginatedServices = allServices.Skip(offset).Take(pageSize).ToList();

        var serviceOutputs = paginatedServices.Select(s => new BarbershopServiceOutput(
            s.Id,
            s.Name,
            s.Description,
            s.DurationMinutes,
            s.Price,
            s.IsActive)).ToList();

        _logger.LogInformation("Listed {Count} services out of {TotalCount} total", serviceOutputs.Count, totalCount);

        return new PaginatedBarbershopServicesOutput(serviceOutputs, totalCount, page, pageSize);
    }
}