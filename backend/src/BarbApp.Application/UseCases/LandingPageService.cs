using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class LandingPageService : ILandingPageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IBarbershopServiceRepository _barbershopServiceRepository;
    private readonly ILogger<LandingPageService> _logger;

    public LandingPageService(
        IUnitOfWork unitOfWork,
        IBarbershopRepository barbershopRepository,
        IBarbershopServiceRepository barbershopServiceRepository,
        ILogger<LandingPageService> logger)
    {
        _unitOfWork = unitOfWork;
        _barbershopRepository = barbershopRepository;
        _barbershopServiceRepository = barbershopServiceRepository;
        _logger = logger;
    }

    public async Task<LandingPageConfigOutput> CreateAsync(
        Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating landing page for barbershop {BarbershopId}", barbershopId);

        var exists = await _unitOfWork.LandingPageConfigs
            .ExistsForBarbershopAsync(barbershopId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Landing page already exists for barbershop {BarbershopId}", barbershopId);
            throw new InvalidOperationException("Landing page already exists for this barbershop");
        }

        var barbershop = await GetBarbershopOrThrowAsync(barbershopId, cancellationToken);

        var config = LandingPageConfig.Create(
            barbershopId: barbershopId,
            templateId: 1,
            whatsappNumber: FormatPhoneForWhatsapp(barbershop.Phone ?? string.Empty),
            openingHours: "Segunda a Sábado: 09:00 - 19:00");

        await _unitOfWork.LandingPageConfigs.InsertAsync(config, cancellationToken);

        var barbershopServices = await GetActiveBarbershopServicesAsync(barbershopId, cancellationToken);

        var displayOrder = 1;
        foreach (var service in barbershopServices)
        {
            var landingPageService = Domain.Entities.LandingPageService.Create(
                config.Id,
                service.Id,
                displayOrder++,
                true);

            await _unitOfWork.LandingPageServices.InsertAsync(landingPageService, cancellationToken);
        }

        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation(
            "Landing page created successfully for barbershop {BarbershopId}. Config ID: {ConfigId}",
            barbershopId,
            config.Id);

        return await GetByBarbershopIdAsync(barbershopId, cancellationToken);
    }

    public async Task<LandingPageConfigOutput> GetByBarbershopIdAsync(
        Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.LandingPageConfigs
            .GetByBarbershopIdWithServicesAsync(barbershopId, cancellationToken);

        if (config == null)
        {
            throw new KeyNotFoundException("Landing page not found");
        }

        return MapToConfigOutput(config);
    }

    public async Task<PublicLandingPageOutput> GetPublicByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required", nameof(code));
        }

        var config = await _unitOfWork.LandingPageConfigs
            .GetPublicByCodeAsync(code, cancellationToken);

        if (config == null)
        {
            throw new KeyNotFoundException("Landing page not found");
        }

        return MapToPublicOutput(config);
    }

    public async Task UpdateConfigAsync(
        Guid barbershopId,
        UpdateLandingPageInput input,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating landing page configuration. Barbershop: {BarbershopId}", barbershopId);

        var config = await _unitOfWork.LandingPageConfigs
            .GetByBarbershopIdAsync(barbershopId, cancellationToken);

        if (config == null)
        {
            throw new KeyNotFoundException("Landing page not found");
        }

        config.Update(
            templateId: input.TemplateId,
            logoUrl: input.LogoUrl,
            aboutText: input.AboutText,
            openingHours: input.OpeningHours,
            instagramUrl: input.InstagramUrl,
            facebookUrl: input.FacebookUrl,
            whatsappNumber: input.WhatsappNumber);

        await _unitOfWork.LandingPageConfigs.UpdateAsync(config, cancellationToken);

        if (input.Services != null && input.Services.Count > 0)
        {
            await UpdateServicesAsync(barbershopId, input.Services, cancellationToken);
        }

        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Landing page updated successfully. Barbershop: {BarbershopId}", barbershopId);
    }

    public async Task UpdateServicesAsync(
        Guid barbershopId,
        List<ServiceDisplayInput> services,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.LandingPageConfigs
            .GetByBarbershopIdAsync(barbershopId, cancellationToken);

        if (config == null)
        {
            throw new KeyNotFoundException("Landing page not found");
        }

        if (!services.Any(s => s.IsVisible))
        {
            throw new InvalidOperationException("At least one service must be visible");
        }

        await _unitOfWork.LandingPageServices.DeleteByLandingPageIdAsync(config.Id, cancellationToken);

        foreach (var serviceInput in services)
        {
            var landingPageService = Domain.Entities.LandingPageService.Create(
                config.Id,
                serviceInput.ServiceId,
                serviceInput.DisplayOrder,
                serviceInput.IsVisible);

            await _unitOfWork.LandingPageServices.InsertAsync(landingPageService, cancellationToken);
        }

        await _unitOfWork.Commit(cancellationToken);
    }

    public async Task<bool> ExistsForBarbershopAsync(
        Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.LandingPageConfigs
            .ExistsForBarbershopAsync(barbershopId, cancellationToken);
    }

    private async Task<Barbershop> GetBarbershopOrThrowAsync(Guid barbershopId, CancellationToken cancellationToken)
    {
        var barbershop = await GetBarbershopByIdAsync(barbershopId, cancellationToken);
        if (barbershop == null)
        {
            throw new KeyNotFoundException("Barbershop not found");
        }
        return barbershop;
    }

    private async Task<Barbershop?> GetBarbershopByIdAsync(Guid barbershopId, CancellationToken cancellationToken)
    {
        return await _barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);
    }

    private async Task<List<BarbershopService>> GetActiveBarbershopServicesAsync(
        Guid barbershopId,
        CancellationToken cancellationToken)
    {
        return await _barbershopServiceRepository.ListAsync(barbershopId, isActive: true, cancellationToken);
    }

    private LandingPageConfigOutput MapToConfigOutput(LandingPageConfig config)
    {
        var barbershop = config.Barbershop != null
            ? new BarbershopBasicInfoOutput(
                config.Barbershop.Id,
                config.Barbershop.Name,
                config.Barbershop.Code.Value,
                config.Barbershop.Address?.ToString() ?? string.Empty)
            : null;

        var services = config.Services
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new LandingPageServiceOutput(
                s.ServiceId,
                s.Service.Name,
                s.Service.Description,
                s.Service.DurationMinutes,
                s.Service.Price,
                s.DisplayOrder,
                s.IsVisible))
            .ToList();

        return new LandingPageConfigOutput(
            config.Id,
            config.BarbershopId,
            config.TemplateId,
            config.LogoUrl,
            config.AboutText,
            config.OpeningHours,
            config.InstagramUrl,
            config.FacebookUrl,
            config.WhatsappNumber,
            config.IsPublished,
            config.UpdatedAt,
            barbershop,
            services);
    }

    private static string FormatPhoneForWhatsapp(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        // Remove all non-numeric characters
        var cleaned = new string(phone.Where(char.IsDigit).ToArray());

        // If it's a Brazilian phone number (10 or 11 digits), add country code
        if (cleaned.Length == 10 || cleaned.Length == 11)
        {
            return $"+55{cleaned}";
        }

        // If it already has country code, return as is
        if (cleaned.StartsWith("55") && cleaned.Length == 12 || cleaned.Length == 13)
        {
            return $"+{cleaned}";
        }

        // For other cases, return cleaned version
        return cleaned;
    }

    private PublicLandingPageOutput MapToPublicOutput(LandingPageConfig config)
    {
        var barbershopInfo = new BarbershopPublicInfoOutput(
            config.Barbershop.Id,
            config.Barbershop.Name,
            config.Barbershop.Code.Value,
            config.Barbershop.Address?.ToString() ?? string.Empty);

        var services = config.Services
            .Where(s => s.IsVisible)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PublicServiceInfoOutput(
                s.Service.Id,
                s.Service.Name,
                s.Service.Description,
                s.Service.DurationMinutes,
                s.Service.Price))
            .ToList();

        var landingPageInfo = new LandingPagePublicInfoOutput(
            config.TemplateId,
            config.LogoUrl,
            config.AboutText,
            config.OpeningHours,
            config.InstagramUrl,
            config.FacebookUrl,
            config.WhatsappNumber,
            services);

        return new PublicLandingPageOutput(barbershopInfo, landingPageInfo);
    }
}
