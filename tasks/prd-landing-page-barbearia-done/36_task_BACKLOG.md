# Task 36.0 - Refatorar Upload de Logo para Cloudflare R2

**Status:** 📋 BACKLOG  
**Fase:** 1 - Correções Críticas de Funcionalidade  
**Prioridade:** Alta  
**Estimativa:** 4-6 horas  
**Depende de:** Task 35 (Bug #4 - Upload básico funcional)

## 📋 Objetivo

Refatorar o sistema de upload de logos da landing page para usar **Cloudflare R2 Object Storage** ao invés de salvar arquivos localmente no container, garantindo persistência, escalabilidade e CDN integrado.

## 🎯 Justificativa

### Problemas da Implementação Atual (LocalLogoUploadService)

1. **❌ Perda de Dados**: Logos salvos em `/uploads/logos/` dentro do container são perdidos ao reiniciar
2. **❌ Não Escalável**: Em ambientes multi-container (Docker Swarm, K8s), cada container tem seu próprio filesystem
3. **❌ Sem CDN**: Imagens servidas diretamente pelo backend ASP.NET (performance ruim)
4. **❌ Sem Backup**: Arquivos não têm redundância ou backup automático
5. **❌ Volume Management**: Precisa configurar volumes Docker para persistência

### Vantagens do Cloudflare R2

1. **✅ S3-Compatible**: API compatível com AWS S3
2. **✅ Sem Taxas de Egress**: Transferência de dados grátis (diferente de AWS S3)
3. **✅ CDN Integrado**: Cloudflare CDN automático
4. **✅ Alta Disponibilidade**: 99.9% SLA
5. **✅ Escalável**: Armazenamento ilimitado
6. **✅ Custo Baixo**: $0.015/GB armazenamento
7. **✅ Backup Automático**: Redundância multi-região

## 📝 Escopo da Task

### Backend (.NET)

#### 1. Instalar Dependências NuGet

```bash
cd backend/src/BarbApp.Application
dotnet add package AWSSDK.S3
dotnet add package AWSSDK.Core
```

#### 2. Criar R2StorageService

**Arquivo:** `/backend/src/BarbApp.Infrastructure/Services/R2StorageService.cs`

```csharp
public interface IR2StorageService
{
    Task<string> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default);
    
    Task<Stream> DownloadFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default);
    
    string GetPublicUrl(string fileKey);
}

public class R2StorageService : IR2StorageService
{
    private readonly AmazonS3Client _s3Client;
    private readonly R2StorageOptions _options;
    
    public R2StorageService(IOptions<R2StorageOptions> options)
    {
        _options = options.Value;
        
        var config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint,
            ForcePathStyle = true,
            SignatureVersion = "4"
        };
        
        var credentials = new BasicAWSCredentials(
            _options.AccessKeyId, 
            _options.SecretAccessKey
        );
        
        _s3Client = new AmazonS3Client(credentials, config);
    }
    
    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var key = GenerateUniqueKey(fileName);
        
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };
        
        await _s3Client.PutObjectAsync(request, cancellationToken);
        
        return GetPublicUrl(key);
    }
    
    public async Task<bool> DeleteFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };
            
            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public string GetPublicUrl(string fileKey)
    {
        return $"{_options.PublicUrl}/{fileKey}";
    }
    
    private string GenerateUniqueKey(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueId = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        
        return $"logos/{timestamp}/{uniqueId}{extension}";
    }
}
```

#### 3. Configuração (appsettings.json)

```json
{
  "R2Storage": {
    "Endpoint": "https://<account-id>.r2.cloudflarestorage.com",
    "BucketName": "barbapp-assets",
    "PublicUrl": "https://assets.barbapp.com",
    "AccessKeyId": "<R2_ACCESS_KEY_ID>",
    "SecretAccessKey": "<R2_SECRET_ACCESS_KEY>"
  }
}
```

#### 4. Refatorar R2LogoUploadService

**Arquivo:** `/backend/src/BarbApp.Application/Services/R2LogoUploadService.cs`

```csharp
public class R2LogoUploadService : ILogoUploadService
{
    private readonly IR2StorageService _r2Storage;
    private readonly IImageProcessor _imageProcessor;
    private readonly ILandingPageRepository _repository;
    private readonly ILogger<R2LogoUploadService> _logger;

    public async Task<string> UploadLogoAsync(
        Guid barbershopId, 
        IFormFile file,
        CancellationToken cancellationToken)
    {
        // 1. Validate file
        ValidateFile(file);
        
        // 2. Process image (resize, optimize)
        using var processedStream = await ProcessImageAsync(file);
        
        // 3. Upload to R2
        var fileName = $"barbershop_{barbershopId}_logo{Path.GetExtension(file.FileName)}";
        var logoUrl = await _r2Storage.UploadFileAsync(
            processedStream,
            fileName,
            file.ContentType,
            cancellationToken
        );
        
        // 4. Update database
        var landingPage = await _repository.GetByBarbershopIdAsync(barbershopId, cancellationToken);
        if (landingPage != null)
        {
            // Delete old logo if exists
            if (!string.IsNullOrEmpty(landingPage.LogoUrl))
            {
                await DeleteOldLogoAsync(landingPage.LogoUrl);
            }
            
            landingPage.UpdateLogo(logoUrl);
            await _repository.UpdateAsync(landingPage, cancellationToken);
        }
        
        return logoUrl;
    }
    
    private async Task<Stream> ProcessImageAsync(IFormFile file)
    {
        using var inputStream = file.OpenReadStream();
        var processedImage = await _imageProcessor.ResizeAsync(
            inputStream,
            300, 300,
            ImageFormat.Png
        );
        
        return processedImage;
    }
    
    private async Task DeleteOldLogoAsync(string logoUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(logoUrl);
            await _r2Storage.DeleteFileAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete old logo: {LogoUrl}", logoUrl);
        }
    }
    
    private string ExtractKeyFromUrl(string url)
    {
        var uri = new Uri(url);
        return uri.AbsolutePath.TrimStart('/');
    }
}
```

#### 5. Registro no DI (Program.cs)

```csharp
// Configurar R2 Storage
builder.Services.Configure<R2StorageOptions>(
    builder.Configuration.GetSection("R2Storage"));

builder.Services.AddSingleton<IR2StorageService, R2StorageService>();

// Trocar LocalLogoUploadService por R2LogoUploadService
builder.Services.AddScoped<ILogoUploadService, R2LogoUploadService>();
```

### Infraestrutura

#### 1. Criar Bucket no Cloudflare R2

```bash
# Via Cloudflare Dashboard:
# 1. Acesse R2 Object Storage
# 2. Crie bucket: "barbapp-assets"
# 3. Configure CORS:
```

**CORS Configuration:**
```json
[
  {
    "AllowedOrigins": ["https://app.barbapp.com", "http://localhost:3000"],
    "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
    "AllowedHeaders": ["*"],
    "ExposeHeaders": ["ETag"],
    "MaxAgeSeconds": 3600
  }
]
```

#### 2. Criar Custom Domain (Opcional mas Recomendado)

```
Bucket: barbapp-assets
Custom Domain: assets.barbapp.com
→ Adicionar CNAME no DNS: assets.barbapp.com → barbapp-assets.<account-id>.r2.cloudflarestorage.com
```

#### 3. Gerar Access Keys

```
Cloudflare Dashboard → R2 → Settings → API Tokens
→ Create API Token
→ Permissions: Object Read & Write
→ Copiar Access Key ID e Secret Access Key
```

### Variáveis de Ambiente

**Desenvolvimento (.env):**
```bash
R2Storage__Endpoint=https://<account-id>.r2.cloudflarestorage.com
R2Storage__BucketName=barbapp-assets-dev
R2Storage__PublicUrl=https://assets-dev.barbapp.com
R2Storage__AccessKeyId=<DEV_ACCESS_KEY>
R2Storage__SecretAccessKey=<DEV_SECRET_KEY>
```

**Produção (Docker Swarm secrets):**
```bash
docker secret create r2_access_key_id <access_key_id>
docker secret create r2_secret_access_key <secret_access_key>
```

**docker-compose.yml:**
```yaml
services:
  api:
    environment:
      - R2Storage__Endpoint=${R2_ENDPOINT}
      - R2Storage__BucketName=${R2_BUCKET}
      - R2Storage__PublicUrl=${R2_PUBLIC_URL}
    secrets:
      - r2_access_key_id
      - r2_secret_access_key

secrets:
  r2_access_key_id:
    external: true
  r2_secret_access_key:
    external: true
```

### Frontend (React)

**Sem alterações necessárias!** 🎉

O frontend continua usando a mesma API. A mudança é transparente:
- Endpoint continua: `POST /api/admin/landing-pages/{id}/logo`
- Response continua: `{ "logoUrl": "...", "message": "..." }`
- Única diferença: `logoUrl` agora aponta para R2 CDN ao invés de filesystem local

### Testes

#### 1. Testes Unitários (R2StorageService)

**Arquivo:** `/backend/tests/BarbApp.Infrastructure.Tests/Services/R2StorageServiceTests.cs`

```csharp
public class R2StorageServiceTests
{
    [Fact]
    public async Task UploadFileAsync_ShouldReturnPublicUrl()
    {
        // Arrange
        var options = CreateTestOptions();
        var service = new R2StorageService(options);
        var fileStream = CreateTestFileStream();
        
        // Act
        var url = await service.UploadFileAsync(
            fileStream, 
            "test.png", 
            "image/png"
        );
        
        // Assert
        url.Should().StartWith("https://assets.barbapp.com/logos/");
    }
    
    [Fact]
    public async Task DeleteFileAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Test implementation
    }
}
```

#### 2. Testes de Integração

```csharp
public class R2LogoUploadServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task UploadLogo_ShouldUploadToR2AndReturnCdnUrl()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var file = CreateTestFormFile();
        
        // Act
        var logoUrl = await _service.UploadLogoAsync(barbershopId, file);
        
        // Assert
        logoUrl.Should().StartWith("https://assets.barbapp.com/");
        
        // Verify file is accessible via CDN
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(logoUrl);
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

#### 3. Testes E2E (Playwright)

**Arquivo:** `/barbapp-admin/tests/e2e/landing-page-logo-upload.spec.ts`

```typescript
test('should upload logo to R2 and display CDN URL', async ({ page }) => {
  await page.goto('http://localhost:3000/CEB4XAR7/landing-page');
  
  // Upload logo
  const fileInput = page.locator('input[type="file"]');
  await fileInput.setInputFiles('./fixtures/test-logo.png');
  
  // Wait for upload
  await expect(page.locator('text=Logo atualizado com sucesso')).toBeVisible();
  
  // Verify CDN URL
  const logoImg = page.locator('img[alt="Logo preview"]');
  const logoSrc = await logoImg.getAttribute('src');
  
  expect(logoSrc).toContain('assets.barbapp.com');
  
  // Verify logo is accessible
  const response = await page.request.get(logoSrc!);
  expect(response.status()).toBe(200);
});
```

## 🎯 Critérios de Aceitação

### Backend
- [ ] AWSSDK.S3 instalado e configurado
- [ ] R2StorageService implementado e testado
- [ ] R2LogoUploadService substituindo LocalLogoUploadService
- [ ] Configuração de R2 em appsettings.json
- [ ] Secrets configurados em Docker Swarm
- [ ] Logs apropriados para upload/delete
- [ ] Tratamento de erros robusto

### Infraestrutura
- [ ] Bucket "barbapp-assets" criado no Cloudflare R2
- [ ] CORS configurado corretamente
- [ ] Custom domain "assets.barbapp.com" configurado (opcional)
- [ ] Access Keys geradas e armazenadas com segurança
- [ ] DNS configurado (se usar custom domain)

### Testes
- [ ] Testes unitários do R2StorageService (≥80% coverage)
- [ ] Testes de integração do R2LogoUploadService
- [ ] Testes E2E do upload via Playwright
- [ ] Teste de upload de arquivos > 2MB (deve rejeitar)
- [ ] Teste de formatos não suportados (deve rejeitar)
- [ ] Teste de deleção de logo antigo ao fazer upload

### Qualidade
- [ ] Código segue padrões do projeto
- [ ] Documentação atualizada
- [ ] Variáveis de ambiente documentadas
- [ ] Migration guide criado (para dados existentes)
- [ ] Rollback plan documentado

## 📚 Arquivos a Criar/Modificar

### Novos Arquivos (9)

**Backend:**
1. `/backend/src/BarbApp.Infrastructure/Services/R2StorageService.cs`
2. `/backend/src/BarbApp.Infrastructure/Options/R2StorageOptions.cs`
3. `/backend/src/BarbApp.Application/Services/R2LogoUploadService.cs`
4. `/backend/tests/BarbApp.Infrastructure.Tests/Services/R2StorageServiceTests.cs`
5. `/backend/tests/BarbApp.IntegrationTests/Services/R2LogoUploadServiceTests.cs`

**Documentação:**
6. `/docs/cloudflare-r2-setup.md`
7. `/docs/migration-local-to-r2.md`
8. `/backend/appsettings.R2.json` (exemplo de configuração)

**E2E:**
9. `/barbapp-admin/tests/e2e/landing-page-logo-upload.spec.ts`

### Arquivos a Modificar (5)

**Backend:**
1. `/backend/src/BarbApp.API/Program.cs` (registro de DI)
2. `/backend/src/BarbApp.Application/BarbApp.Application.csproj` (adicionar AWSSDK.S3)
3. `/backend/src/BarbApp.API/appsettings.json` (adicionar R2Storage config)
4. `/backend/src/BarbApp.API/appsettings.Development.json`

**Infraestrutura:**
5. `/backend/docker-compose.yml` (adicionar secrets e env vars)

### Arquivos a Remover (2)

1. `/backend/src/BarbApp.Application/Services/LocalLogoUploadService.cs` (deprecar)
2. `/backend/tests/BarbApp.Application.Tests/Services/LocalLogoUploadServiceTests.cs`

## 📦 Dependências

### NuGet Packages
```xml
<PackageReference Include="AWSSDK.S3" Version="3.7.307" />
<PackageReference Include="AWSSDK.Core" Version="3.7.304" />
```

### Cloudflare R2 Account
- Account ID
- R2 API Token (Read & Write)
- Bucket criado

## 🚀 Plano de Implementação

### Fase 1: Setup Inicial (1h)
1. Criar bucket no Cloudflare R2
2. Configurar CORS
3. Gerar Access Keys
4. Adicionar secrets no Docker Swarm (produção)

### Fase 2: Implementação Backend (2-3h)
1. Instalar AWSSDK.S3
2. Criar R2StorageOptions
3. Implementar R2StorageService
4. Implementar R2LogoUploadService
5. Atualizar Program.cs (DI)
6. Atualizar appsettings.json

### Fase 3: Testes (1-2h)
1. Testes unitários R2StorageService
2. Testes integração R2LogoUploadService
3. Testes E2E com Playwright
4. Testes manuais em desenvolvimento

### Fase 4: Documentação e Deploy (1h)
1. Documentar setup do R2
2. Criar migration guide
3. Atualizar environment-variables.md
4. Deploy em staging
5. Validação em produção

## 🔄 Migration Strategy

### Para Dados Existentes (se houver logos no filesystem local)

**Script de Migração:**
```bash
#!/bin/bash
# migrate-logos-to-r2.sh

# 1. Listar todos os logos atuais
LOGOS_DIR="/app/uploads/logos"

# 2. Upload para R2
for logo in $LOGOS_DIR/*; do
    filename=$(basename "$logo")
    
    # Upload usando AWS CLI
    aws s3 cp "$logo" \
        "s3://barbapp-assets/logos/$filename" \
        --endpoint-url "https://<account-id>.r2.cloudflarestorage.com" \
        --acl public-read
    
    # Atualizar URL no banco de dados
    NEW_URL="https://assets.barbapp.com/logos/$filename"
    # SQL update aqui
done
```

### Rollback Plan

Se houver problemas com R2:

1. **Reverter código:**
   ```bash
   git revert <commit-hash>
   ```

2. **Restaurar DI:**
   ```csharp
   services.AddScoped<ILogoUploadService, LocalLogoUploadService>();
   ```

3. **Restaurar variáveis de ambiente** (remover R2Storage)

## 💰 Custo Estimado (Cloudflare R2)

### Pricing (2025)
- **Storage:** $0.015/GB/mês
- **Class A Operations (write):** $4.50/million requests
- **Class B Operations (read):** $0.36/million requests
- **Egress:** **$0** (FREE! 🎉)

### Estimativa para BarbApp
- **100 barbearias** × 1 logo (300KB) = 30MB ≈ **$0.0005/mês**
- **1,000 uploads/mês** ≈ **$0.005/mês**
- **100,000 visualizações/mês** ≈ **$0.036/mês**

**Total estimado: ~$0.05/mês** (praticamente grátis!)

## 🔗 Referências

- [Cloudflare R2 Documentation](https://developers.cloudflare.com/r2/)
- [AWS SDK for .NET - S3](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/welcome.html)
- [R2 API Compatibility](https://developers.cloudflare.com/r2/api/s3/api/)
- [Cloudflare R2 Pricing](https://developers.cloudflare.com/r2/pricing/)

## ⚠️ Considerações de Segurança

1. **Secrets Management:**
   - ❌ NUNCA commitar Access Keys no Git
   - ✅ Usar Docker Secrets em produção
   - ✅ Usar .env em desenvolvimento (no .gitignore)

2. **CORS Configuration:**
   - Limitar origens permitidas (não usar `*`)
   - Validar headers

3. **File Validation:**
   - Manter validações de tamanho e tipo
   - Scan de malware (opcional, futuro)

4. **Access Control:**
   - Logos são públicos (S3CannedACL.PublicRead)
   - Considerar signed URLs para conteúdo privado (futuro)

## 📊 Métricas de Sucesso

- [ ] 100% dos uploads funcionando via R2
- [ ] 0 erros 500 relacionados a upload
- [ ] Tempo de upload < 2 segundos (95th percentile)
- [ ] CDN response time < 100ms
- [ ] 0 perda de dados após restarts de container

---

**Criado em:** 2025-10-22  
**Prioridade:** Alta (bloqueia escalabilidade)  
**Relacionado a:** Bug #4, Task 35  
**Próxima task:** Task 37 (Bugs #1, #2, #3 - Landing Page Pública)
