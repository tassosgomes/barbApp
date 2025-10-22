using BarbApp.Application.Interfaces;
using BarbApp.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Serviço de upload de logos usando Cloudflare R2 Object Storage
/// </summary>
public class R2LogoUploadService : ILogoUploadService
{
    private readonly IR2StorageService _r2Storage;
    private readonly IImageProcessor _imageProcessor;
    private readonly ILogger<R2LogoUploadService> _logger;
    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".svg", ".webp" };

    public R2LogoUploadService(
        IR2StorageService r2Storage,
        IImageProcessor imageProcessor,
        ILogger<R2LogoUploadService> logger)
    {
        _r2Storage = r2Storage;
        _imageProcessor = imageProcessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<string>> UploadLogoAsync(
        Guid barbershopId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando upload de logo para R2. BarbershopId: {BarbershopId}, FileName: {FileName}, Size: {Size}",
                barbershopId, file.FileName, file.Length);

            // Validar arquivo
            if (!IsValidImageFile(file))
            {
                _logger.LogWarning(
                    "Arquivo inválido para upload. BarbershopId: {BarbershopId}, FileName: {FileName}",
                    barbershopId, file.FileName);
                return Result<string>.Failure("Arquivo inválido. Use JPG, PNG, SVG ou WebP com no máximo 2MB");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"barbershop_{barbershopId}_logo{fileExtension}";

            Stream uploadStream;
            string contentType;

            // SVG não precisa de processamento
            if (fileExtension == ".svg")
            {
                _logger.LogInformation("Processando SVG sem redimensionamento. BarbershopId: {BarbershopId}", barbershopId);
                uploadStream = file.OpenReadStream();
                contentType = "image/svg+xml";
            }
            else
            {
                _logger.LogInformation("Processando e redimensionando imagem. BarbershopId: {BarbershopId}", barbershopId);

                // Processar imagem (redimensionar para 300x300)
                var inputStream = file.OpenReadStream();
                uploadStream = await _imageProcessor.ProcessImageAsync(
                    inputStream,
                    cancellationToken);

                contentType = fileExtension switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".webp" => "image/webp",
                    _ => file.ContentType
                };
            }

            // Upload para R2
            string logoUrl;
            using (uploadStream)
            {
                logoUrl = await _r2Storage.UploadFileAsync(
                    uploadStream,
                    fileName,
                    contentType,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Logo enviado com sucesso para R2. BarbershopId: {BarbershopId}, URL: {LogoUrl}",
                barbershopId, logoUrl);

            return Result<string>.Success(logoUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao fazer upload do logo para R2. BarbershopId: {BarbershopId}",
                barbershopId);
            return Result<string>.Failure($"Erro ao fazer upload do logo: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteLogoAsync(
        string logoUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando remoção de logo do R2: {LogoUrl}", logoUrl);

            if (string.IsNullOrEmpty(logoUrl))
            {
                _logger.LogWarning("URL de logo vazia fornecida para remoção");
                return Result.Success();
            }

            // Extrair key da URL
            // Ex: https://assets.barbapp.com/logos/20251022/abc.png → logos/20251022/abc.png
            var fileKey = ExtractKeyFromUrl(logoUrl);

            var deleted = await _r2Storage.DeleteFileAsync(fileKey, cancellationToken);

            if (deleted)
            {
                _logger.LogInformation("Logo removido com sucesso do R2: {LogoUrl}", logoUrl);
                return Result.Success();
            }
            else
            {
                _logger.LogWarning("Falha ao remover logo do R2 (arquivo pode não existir): {LogoUrl}", logoUrl);
                return Result.Success(); // Não falhamos se o arquivo já não existe
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover logo do R2: {LogoUrl}", logoUrl);
            return Result.Failure($"Erro ao remover logo: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Arquivo nulo ou vazio enviado");
            return false;
        }

        if (file.Length > MaxFileSize)
        {
            _logger.LogWarning(
                "Arquivo muito grande: {Size} bytes. Máximo permitido: {MaxSize} bytes",
                file.Length, MaxFileSize);
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning(
                "Extensão de arquivo não permitida: {Extension}. Extensões permitidas: {Allowed}",
                extension, string.Join(", ", AllowedExtensions));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Extrai a key do arquivo a partir da URL completa
    /// </summary>
    /// <param name="url">URL completa do arquivo</param>
    /// <returns>Key relativa no bucket (ex: logos/20251022/abc.png)</returns>
    private string ExtractKeyFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.TrimStart('/');
            return path;
        }
        catch
        {
            // Se não for uma URL válida, assume que já é uma key
            return url.TrimStart('/');
        }
    }
}
