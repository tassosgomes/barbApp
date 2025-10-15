using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class GetBarberByIdUseCase : IGetBarberByIdUseCase
{
    private readonly IBarberRepository _barberRepository;
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetBarberByIdUseCase> _logger;

    public GetBarberByIdUseCase(
        IBarberRepository barberRepository,
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        ILogger<GetBarberByIdUseCase> logger)
    {
        _barberRepository = barberRepository;
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<BarberOutput> ExecuteAsync(Guid barberId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting barber by ID {BarberId}", barberId);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Get barber
        var barber = await _barberRepository.GetByIdAsync(barberId, cancellationToken);
        if (barber == null || barber.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning("Barber with ID {BarberId} not found or does not belong to barbearia {BarbeariaId}", barberId, barbeariaId);
            throw new BarberNotFoundException(barberId);
        }

        // Load services
        var services = new List<BarbershopServiceOutput>();
        if (barber.ServiceIds.Any())
        {
            var allServices = await _serviceRepository.ListAsync(barbeariaId.Value, isActive: true, cancellationToken);
            services = allServices
                .Where(s => barber.ServiceIds.Contains(s.Id))
                .Select(s => new BarbershopServiceOutput(s.Id, s.Name, s.Description, s.DurationMinutes, s.Price, s.IsActive))
                .ToList();
        }

        _logger.LogInformation("Barber {BarberId} retrieved successfully", barberId);

        return new BarberOutput(
            barber.Id,
            barber.Name,
            barber.Email,
            FormatPhone(barber.Phone),
            services,
            barber.IsActive,
            barber.CreatedAt);
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