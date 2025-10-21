using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repositório para gerenciamento de configurações de landing pages
/// </summary>
public interface ILandingPageConfigRepository
{
    /// <summary>
    /// Obtém configuração da landing page por ID da barbearia
    /// </summary>
    Task<LandingPageConfig?> GetByBarbershopIdAsync(Guid barbershopId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém configuração da landing page por código da barbearia
    /// </summary>
    Task<LandingPageConfig?> GetByBarbershopCodeAsync(string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém configuração da landing page com serviços carregados
    /// </summary>
    Task<LandingPageConfig?> GetByBarbershopIdWithServicesAsync(Guid barbershopId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém landing page pública por código da barbearia (apenas se publicada)
    /// </summary>
    Task<LandingPageConfig?> GetPublicByCodeAsync(string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe landing page para a barbearia
    /// </summary>
    Task<bool> ExistsForBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Insere nova configuração de landing page
    /// </summary>
    Task InsertAsync(LandingPageConfig config, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza configuração de landing page
    /// </summary>
    Task UpdateAsync(LandingPageConfig config, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove configuração de landing page
    /// </summary>
    Task DeleteAsync(LandingPageConfig config, CancellationToken cancellationToken = default);
}
