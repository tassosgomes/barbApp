---
status: pending
parallelizable: true
blocked_by: ["4.0"]
---

<task_context>
<domain>backend/infra</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>5.0</unblocks>
</task_context>

# Tarefa 7.0: Serviço de Upload e Processamento de Logos

## Visão Geral

Implementar serviço para upload, validação, redimensionamento e armazenamento de logos das barbearias para as landing pages. Inclui integração com sistema de storage (local ou cloud).

<requirements>
- Upload de arquivos (JPG, PNG, SVG)
- Validação de tipo e tamanho (máx 2MB)
- Redimensionamento automático para 300x300px
- Armazenamento em filesystem ou S3/Cloudinary
- Geração de URL pública para acesso
- Remoção de logos antigos
- Logging de operações
</requirements>

## Subtarefas

- [ ] 7.1 Criar interface `ILogoUploadService`
- [ ] 7.2 Implementar `LocalLogoUploadService` (filesystem)
- [ ] 7.3 Adicionar validação de arquivo
- [ ] 7.4 Implementar redimensionamento com ImageSharp
- [ ] 7.5 Gerar nomes únicos de arquivo
- [ ] 7.6 Implementar endpoint POST de upload
- [ ] 7.7 Adicionar testes unitários e integração

## Detalhes de Implementação

### Interface: ILogoUploadService.cs

```csharp
namespace BarbApp.Application.Interfaces
{
    public interface ILogoUploadService
    {
        Task<Result<string>> UploadLogoAsync(Guid barbershopId, IFormFile file, CancellationToken cancellationToken = default);
        Task<Result> DeleteLogoAsync(string logoUrl, CancellationToken cancellationToken = default);
        bool IsValidImageFile(IFormFile file);
    }
}
```

### Implementação: LocalLogoUploadService.cs

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BarbApp.Infrastructure.Services
{
    public class LocalLogoUploadService : ILogoUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalLogoUploadService> _logger;
        private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".svg" };
        private const int TargetSize = 300;
        
        public LocalLogoUploadService(IWebHostEnvironment environment, ILogger<LocalLogoUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }
        
        public async Task<Result<string>> UploadLogoAsync(
            Guid barbershopId, 
            IFormFile file, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsValidImageFile(file))
                {
                    return Result<string>.Failure("Arquivo inválido");
                }
                
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                Directory.CreateDirectory(uploadsPath);
                
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{barbershopId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                
                // SVG não precisa de redimensionamento
                if (fileExtension == ".svg")
                {
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream, cancellationToken);
                }
                else
                {
                    // Redimensionar imagem
                    using var image = await Image.LoadAsync(file.OpenReadStream(), cancellationToken);
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(TargetSize, TargetSize),
                        Mode = ResizeMode.Crop
                    }));
                    
                    await image.SaveAsync(filePath, cancellationToken);
                }
                
                var logoUrl = $"/uploads/logos/{fileName}";
                
                _logger.LogInformation("Logo uploaded successfully for barbershop {BarbershopId}. URL: {LogoUrl}", barbershopId, logoUrl);
                
                return Result<string>.Success(logoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo for barbershop {BarbershopId}", barbershopId);
                return Result<string>.Failure("Erro ao fazer upload do logo");
            }
        }
        
        public async Task<Result> DeleteLogoAsync(string logoUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(logoUrl))
                {
                    return Result.Success();
                }
                
                var filePath = Path.Combine(_environment.WebRootPath, logoUrl.TrimStart('/'));
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Logo deleted: {LogoUrl}", logoUrl);
                }
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting logo: {LogoUrl}", logoUrl);
                return Result.Failure("Erro ao deletar logo");
            }
        }
        
        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            
            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning("File too large: {Size} bytes", file.Length);
                return false;
            }
            
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file extension: {Extension}", extension);
                return false;
            }
            
            return true;
        }
    }
}
```

### Endpoint no Controller: LandingPageController.cs

```csharp
/// <summary>
/// Faz upload do logo da landing page
/// </summary>
[HttpPost("{barbershopId:guid}/logo")]
[ProducesResponseType(typeof(LogoUploadResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[RequestSizeLimit(2 * 1024 * 1024)] // 2MB
public async Task<IActionResult> UploadLogo(
    Guid barbershopId, 
    IFormFile logo,
    [FromServices] ILogoUploadService logoUploadService,
    CancellationToken cancellationToken)
{
    if (!await IsAuthorizedForBarbershop(barbershopId))
    {
        return Forbid();
    }
    
    if (logo == null)
    {
        return BadRequest(new { error = "Arquivo não fornecido" });
    }
    
    // Upload do arquivo
    var uploadResult = await logoUploadService.UploadLogoAsync(barbershopId, logo, cancellationToken);
    
    if (!uploadResult.IsSuccess)
    {
        return BadRequest(new { error = uploadResult.Error });
    }
    
    // Atualizar configuração da landing page
    var config = await _landingPageService.GetByBarbershopIdAsync(barbershopId, cancellationToken);
    if (config.IsSuccess)
    {
        // Deletar logo antigo se existir
        if (!string.IsNullOrEmpty(config.Data.LogoUrl))
        {
            await logoUploadService.DeleteLogoAsync(config.Data.LogoUrl, cancellationToken);
        }
        
        // Atualizar com novo logo
        await _landingPageService.UpdateConfigAsync(
            barbershopId, 
            new UpdateLandingPageRequest { LogoUrl = uploadResult.Data },
            cancellationToken);
    }
    
    return Ok(new { logoUrl = uploadResult.Data });
}

public class LogoUploadResponse
{
    public string LogoUrl { get; set; } = string.Empty;
}
```

## Sequenciamento

- **Bloqueado por**: 4.0 (Serviços de Domínio)
- **Desbloqueia**: 5.0 (endpoint completo de upload)
- **Paralelizável**: Sim

## Critérios de Sucesso

- [ ] Upload funcionando para JPG, PNG, SVG
- [ ] Validação de tamanho e tipo efetiva
- [ ] Redimensionamento automático funcionando
- [ ] URLs públicas geradas corretamente
- [ ] Remoção de logos antigos funcionando
- [ ] Testes cobrindo casos de sucesso e erro
- [ ] Performance < 500ms para upload
