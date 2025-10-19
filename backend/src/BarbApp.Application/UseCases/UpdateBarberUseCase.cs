using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class UpdateBarberUseCase : IUpdateBarberUseCase
{
    private readonly IBarberRepository _barberRepository;
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBarberUseCase> _logger;

    public UpdateBarberUseCase(
        IBarberRepository barberRepository,
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<UpdateBarberUseCase> logger)
    {
        _barberRepository = barberRepository;
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarberOutput> ExecuteAsync(UpdateBarberInput input, CancellationToken cancellationToken)
    {
        var maskedPhone = input.Phone != null ? MaskPhone(input.Phone) : "***";
        _logger.LogInformation("Starting update of barber with ID {BarberId} and phone {MaskedPhone}", input.Id, maskedPhone);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            _logger.LogError("Failed to update barber: Tenant context not defined");
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Get existing barber
        var barber = await _barberRepository.GetByIdAsync(input.Id, cancellationToken);
        if (barber == null || barber.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning("Barber with ID {BarberId} not found or does not belong to barbearia {BarbeariaId}", input.Id, barbeariaId);
            throw new BarberNotFoundException(input.Id);
        }

        // Update barber
        var name = input.Name ?? barber.Name;
        var phone = input.Phone ?? barber.Phone;
        var serviceIds = input.ServiceIds ?? barber.ServiceIds;
        
        barber.Update(name, phone, serviceIds);

        await _barberRepository.UpdateAsync(barber, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Barber updated successfully with ID {BarberId}", barber.Id);

        // Load services for output
        var services = new List<BarbershopServiceOutput>();
        if (barber.ServiceIds.Any())
        {
            var allServices = await _serviceRepository.ListAsync(barbeariaId.Value, isActive: true, cancellationToken);
            services = allServices
                .Where(s => barber.ServiceIds.Contains(s.Id))
                .Select(s => new BarbershopServiceOutput(s.Id, s.Name, s.Description, s.DurationMinutes, s.Price, s.IsActive))
                .ToList();
        }

        return new BarberOutput(
            barber.Id,
            barber.Name,
            barber.Email,
            FormatPhone(barber.Phone),
            services,
            barber.IsActive,
            barber.CreatedAt);
    }

    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4)
            return "***";

        // Mask all but last 4 digits
        var visible = phone.Length >= 4 ? phone[^4..] : phone;
        var masked = new string('*', phone.Length - visible.Length) + visible;
        return masked;
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