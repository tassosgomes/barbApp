using BarbApp.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace BarbApp.Application.Interfaces;

/// <summary>
/// Interface para serviço de upload e processamento de logos
/// </summary>
public interface ILogoUploadService
{
    /// <summary>
    /// Faz upload e processa um logo para uma barbearia
    /// </summary>
    /// <param name="barbershopId">ID da barbearia</param>
    /// <param name="file">Arquivo de imagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>URL do logo processado</returns>
    Task<Result<string>> UploadLogoAsync(
        Guid barbershopId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um logo existente
    /// </summary>
    /// <param name="logoUrl">URL do logo a ser removido</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> DeleteLogoAsync(
        string logoUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida se um arquivo é uma imagem válida
    /// </summary>
    /// <param name="file">Arquivo a ser validado</param>
    /// <returns>True se válido, false caso contrário</returns>
    bool IsValidImageFile(IFormFile file);
}