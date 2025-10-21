using BarbApp.Domain.Entities;

namespace BarbApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repositório para gerenciamento de serviços exibidos na landing page
/// </summary>
public interface ILandingPageServiceRepository
{
    /// <summary>
    /// Obtém todos os serviços de uma landing page ordenados por DisplayOrder
    /// </summary>
    Task<List<LandingPageService>> GetByLandingPageIdAsync(Guid landingPageId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove todos os serviços de uma landing page
    /// </summary>
    Task DeleteByLandingPageIdAsync(Guid landingPageId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe relação entre landing page e serviço
    /// </summary>
    Task<bool> ExistsAsync(Guid landingPageId, Guid serviceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Insere novo serviço na landing page
    /// </summary>
    Task InsertAsync(LandingPageService service, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Insere múltiplos serviços na landing page
    /// </summary>
    Task InsertRangeAsync(IEnumerable<LandingPageService> services, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza serviço da landing page
    /// </summary>
    Task UpdateAsync(LandingPageService service, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove serviço da landing page
    /// </summary>
    Task DeleteAsync(LandingPageService service, CancellationToken cancellationToken = default);
}
