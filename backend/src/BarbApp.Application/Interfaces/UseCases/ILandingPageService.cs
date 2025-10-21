using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

/// <summary>
/// Interface para serviços de domínio da Landing Page
/// </summary>
public interface ILandingPageService
{
    /// <summary>
    /// Cria uma nova landing page para a barbearia
    /// </summary>
    Task<LandingPageConfigOutput> CreateAsync(
        Guid barbershopId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém configuração da landing page por ID da barbearia
    /// </summary>
    Task<LandingPageConfigOutput> GetByBarbershopIdAsync(
        Guid barbershopId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém landing page pública por código da barbearia
    /// </summary>
    Task<PublicLandingPageOutput> GetPublicByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza configuração da landing page
    /// </summary>
    Task UpdateConfigAsync(
        Guid barbershopId,
        UpdateLandingPageInput input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza serviços exibidos na landing page
    /// </summary>
    Task UpdateServicesAsync(
        Guid barbershopId,
        List<ServiceDisplayInput> services,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe landing page para a barbearia
    /// </summary>
    Task<bool> ExistsForBarbershopAsync(
        Guid barbershopId,
        CancellationToken cancellationToken = default);
}
