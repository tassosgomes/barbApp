using BarbApp.Application.Interfaces;
using BarbApp.Domain.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Serviço de upload de logos usando armazenamento local em filesystem
/// </summary>
public class LocalLogoUploadService : ILogoUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalLogoUploadService> _logger;
    private readonly IImageProcessor _imageProcessor;
    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".svg" };
    private const int TargetSize = 300;

    public LocalLogoUploadService(
        IWebHostEnvironment environment,
        ILogger<LocalLogoUploadService> logger,
        IImageProcessor imageProcessor)
    {
        _environment = environment;
        _logger = logger;
        _imageProcessor = imageProcessor;
    }

    /// <inheritdoc />
    public async Task<Result<string>> UploadLogoAsync(
        Guid barbershopId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando upload de logo para barbearia {BarbershopId}", barbershopId);

            if (!IsValidImageFile(file))
            {
                _logger.LogWarning("Arquivo inválido enviado para barbearia {BarbershopId}", barbershopId);
                return Result<string>.Failure("Arquivo inválido. Use JPG, PNG ou SVG com no máximo 2MB");
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "logos");
            Directory.CreateDirectory(uploadsPath);

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{barbershopId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // SVG não precisa de redimensionamento
            if (fileExtension == ".svg")
            {
                _logger.LogInformation("Salvando arquivo SVG para barbearia {BarbershopId}", barbershopId);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Processando e redimensionando imagem para barbearia {BarbershopId}", barbershopId);
                // Redimensionar imagem usando ImageSharp
                await _imageProcessor.ProcessAndSaveImageAsync(file.OpenReadStream(), filePath, cancellationToken);
            }

            var logoUrl = $"/uploads/logos/{fileName}";

            _logger.LogInformation("Logo enviado com sucesso para barbearia {BarbershopId}. URL: {LogoUrl}",
                barbershopId, logoUrl);

            return Result<string>.Success(logoUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do logo para barbearia {BarbershopId}", barbershopId);
            return Result<string>.Failure("Erro ao fazer upload do logo");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteLogoAsync(
        string logoUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando remoção de logo: {LogoUrl}", logoUrl);

            if (string.IsNullOrEmpty(logoUrl))
            {
                _logger.LogWarning("URL de logo vazia fornecida para remoção");
                return Result.Success();
            }

            var filePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), logoUrl.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Logo removido com sucesso: {LogoUrl}", logoUrl);
            }
            else
            {
                _logger.LogWarning("Arquivo de logo não encontrado: {FilePath}", filePath);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover logo: {LogoUrl}", logoUrl);
            return Result.Failure("Erro ao remover logo");
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
            _logger.LogWarning("Arquivo muito grande: {Size} bytes. Máximo permitido: {MaxSize} bytes",
                file.Length, MaxFileSize);
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Extensão de arquivo não permitida: {Extension}. Extensões permitidas: {Allowed}",
                extension, string.Join(", ", AllowedExtensions));
            return false;
        }

        return true;
    }
}