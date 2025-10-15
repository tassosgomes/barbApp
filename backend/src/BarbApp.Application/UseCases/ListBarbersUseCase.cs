using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListBarbersUseCase : IListBarbersUseCase
{
    private readonly IBarberRepository _barberRepository;
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListBarbersUseCase> _logger;

    public ListBarbersUseCase(
        IBarberRepository barberRepository,
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        ILogger<ListBarbersUseCase> logger)
    {
        _barberRepository = barberRepository;
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<PaginatedBarbersOutput> ExecuteAsync(
        bool? isActive,
        string? searchName,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing barbers with filters: isActive={IsActive}, searchName={SearchName}, page={Page}, pageSize={PageSize}",
            isActive, searchName, page, pageSize);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var offset = (page - 1) * pageSize;

        // Get barbers
        var barbers = await _barberRepository.ListAsync(
            barbeariaId.Value,
            isActive,
            searchName,
            pageSize,
            offset,
            cancellationToken);

        var totalCount = await _barberRepository.CountAsync(
            barbeariaId.Value,
            isActive,
            searchName,
            cancellationToken);

        // Load services for all barbers
        var allServices = await _serviceRepository.ListAsync(barbeariaId.Value, isActive: true, cancellationToken);
        var servicesById = allServices.ToDictionary(s => s.Id);

        var barberOutputs = barbers.Select(barber =>
        {
            var services = barber.ServiceIds
                .Where(id => servicesById.ContainsKey(id))
                .Select(id => servicesById[id])
                .Select(s => new BarbershopServiceOutput(s.Id, s.Name, s.Description, s.DurationMinutes, s.Price, s.IsActive))
                .ToList();

            return new BarberOutput(
                barber.Id,
                barber.Name,
                barber.Email,
                FormatPhone(barber.Phone),
                services,
                barber.IsActive,
                barber.CreatedAt);
        }).ToList();

        _logger.LogInformation("Listed {Count} barbers out of {TotalCount} total", barberOutputs.Count, totalCount);

        return new PaginatedBarbersOutput(barberOutputs, totalCount, page, pageSize);
    }

    private static string FormatPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 10)
            return phone;

        if (phone.Length == 10)
        {
            // (11) 98765-4321
            return $"({phone.Substring(0, 2)}) {phone.Substring(2, 4)}-{phone.Substring(6)}";
        }
        else if (phone.Length == 11)
        {
            // (11) 98765-4321
            return $"({phone.Substring(0, 2)}) {phone.Substring(2, 5)}-{phone.Substring(7)}";
        }

        return phone;
    }
}