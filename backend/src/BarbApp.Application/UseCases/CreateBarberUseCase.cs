using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class CreateBarberUseCase : ICreateBarberUseCase
{
    private readonly IBarberRepository _barberRepository;
    private readonly IBarbershopServiceRepository _serviceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBarberUseCase> _logger;

    public CreateBarberUseCase(
        IBarberRepository barberRepository,
        IBarbershopServiceRepository serviceRepository,
        ITenantContext tenantContext,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<CreateBarberUseCase> logger)
    {
        _barberRepository = barberRepository;
        _serviceRepository = serviceRepository;
        _tenantContext = tenantContext;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarberOutput> ExecuteAsync(CreateBarberInput input, CancellationToken cancellationToken)
    {
        var maskedPhone = MaskPhone(input.Phone);
        _logger.LogInformation("Starting creation of new barber with email {Email} and phone {MaskedPhone}", input.Email, maskedPhone);

        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            _logger.LogError("Failed to create barber: Tenant context not defined");
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        // Check for duplicate email in the same barbearia
        var existingBarber = await _barberRepository.GetByEmailAsync(barbeariaId.Value, input.Email, cancellationToken);
        if (existingBarber != null)
        {
            _logger.LogWarning("Attempted to create barber with duplicate email {Email} in barbearia {BarbeariaId}", input.Email, barbeariaId);
            throw new DuplicateBarberException(input.Email, barbeariaId.Value);
        }

        // Hash the password
        var passwordHash = _passwordHasher.Hash(input.Password);

        // Create barber
        var barber = Barber.Create(
            barbeariaId.Value,
            input.Name,
            input.Email,
            passwordHash,
            input.Phone,
            input.ServiceIds);

        await _barberRepository.InsertAsync(barber, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        // Increment metrics
        BarbAppMetrics.BarberCreatedCounter.WithLabels(barbeariaId.Value.ToString()).Inc();
        BarbAppMetrics.ActiveBarbersGauge.WithLabels(barbeariaId.Value.ToString()).Inc();

        _logger.LogInformation("Barber created successfully with ID {BarberId} in barbearia {BarbeariaId}", barber.Id, barbeariaId);

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