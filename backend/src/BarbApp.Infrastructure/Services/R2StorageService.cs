using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using BarbApp.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Interface para serviço de armazenamento no Cloudflare R2
/// </summary>
public interface IR2StorageService
{
    /// <summary>
    /// Faz upload de um arquivo para o R2
    /// </summary>
    /// <param name="fileStream">Stream do arquivo</param>
    /// <param name="fileName">Nome do arquivo</param>
    /// <param name="contentType">Tipo de conteúdo (MIME type)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>URL pública do arquivo</returns>
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta um arquivo do R2
    /// </summary>
    /// <param name="fileKey">Chave do arquivo (path relativo no bucket)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se deletado com sucesso</returns>
    Task<bool> DeleteFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Faz download de um arquivo do R2
    /// </summary>
    /// <param name="fileKey">Chave do arquivo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Stream do arquivo</returns>
    Task<Stream> DownloadFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna a URL pública de um arquivo
    /// </summary>
    /// <param name="fileKey">Chave do arquivo</param>
    /// <returns>URL pública completa</returns>
    string GetPublicUrl(string fileKey);
}

/// <summary>
/// Implementação do serviço de armazenamento Cloudflare R2 usando S3 SDK
/// </summary>
public class R2StorageService : IR2StorageService, IDisposable
{
    private readonly AmazonS3Client _s3Client;
    private readonly R2StorageOptions _options;
    private readonly ILogger<R2StorageService> _logger;

    public R2StorageService(
        IOptions<R2StorageOptions> options,
        ILogger<R2StorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Validar configurações
        _options.Validate();

        // Configurar cliente S3 para Cloudflare R2
        var config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint,
            ForcePathStyle = true,
            UseHttp = false,
            Timeout = TimeSpan.FromSeconds(30),
            MaxErrorRetry = 3,
            AuthenticationRegion = "auto" // R2 requer "auto" como região
        };

        var credentials = new BasicAWSCredentials(
            _options.AccessKeyId,
            _options.SecretAccessKey
        );

        _s3Client = new AmazonS3Client(credentials, config);

        _logger.LogInformation(
            "R2StorageService inicializado. Bucket: {BucketName}, Endpoint: {Endpoint}",
            _options.BucketName,
            _options.Endpoint);
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GenerateUniqueKey(fileName);

            _logger.LogInformation(
                "Iniciando upload para R2. FileName: {FileName}, Key: {Key}, ContentType: {ContentType}",
                fileName, key, contentType);

            // R2 não suporta chunked encoding com STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER
            // Solução: Copiar stream para MemoryStream para ter Content-Length conhecido
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = memoryStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead,
                DisablePayloadSigning = true, // Desabilita assinatura de payload para compatibilidade com R2
                DisableDefaultChecksumValidation = true // Desabilita validação de checksum padrão
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            _logger.LogInformation(
                "Upload concluído com sucesso. Key: {Key}, ETag: {ETag}",
                key, response.ETag);

            return GetPublicUrl(key);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao fazer upload para R2. FileName: {FileName}, Message: {Message}",
                fileName, ex.Message);
            throw new InvalidOperationException($"Falha ao fazer upload do arquivo: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deletando arquivo do R2. Key: {Key}", fileKey);

            var request = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);

            _logger.LogInformation("Arquivo deletado com sucesso. Key: {Key}", fileKey);
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogWarning(ex,
                "Erro ao deletar arquivo do R2. Key: {Key}, Message: {Message}",
                fileKey, ex.Message);
            return false;
        }
    }

    public async Task<Stream> DownloadFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Baixando arquivo do R2. Key: {Key}", fileKey);

            var request = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);

            _logger.LogInformation("Arquivo baixado com sucesso. Key: {Key}", fileKey);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao baixar arquivo do R2. Key: {Key}, Message: {Message}",
                fileKey, ex.Message);
            throw new InvalidOperationException($"Falha ao baixar arquivo: {ex.Message}", ex);
        }
    }

    public string GetPublicUrl(string fileKey)
    {
        var publicUrl = _options.PublicUrl.TrimEnd('/');
        var key = fileKey.TrimStart('/');
        return $"{publicUrl}/{key}";
    }

    /// <summary>
    /// Gera uma chave única para o arquivo com estrutura organizada por data
    /// Formato: logos/YYYYMMDD/guid.extensão
    /// </summary>
    private string GenerateUniqueKey(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueId = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");

        return $"logos/{timestamp}/{uniqueId}{extension}";
    }

    public void Dispose()
    {
        _s3Client?.Dispose();
    }
}
