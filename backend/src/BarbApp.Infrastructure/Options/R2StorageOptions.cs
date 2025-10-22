namespace BarbApp.Infrastructure.Options;

/// <summary>
/// Opções de configuração para Cloudflare R2 Object Storage
/// </summary>
public class R2StorageOptions
{
    /// <summary>
    /// URL do endpoint R2 (ex: https://account-id.r2.cloudflarestorage.com)
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Nome do bucket R2 (ex: barbapp-assets)
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// URL pública para acessar os arquivos via CDN
    /// (ex: https://assets.barbapp.com ou https://pub-xyz.r2.dev)
    /// </summary>
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// Access Key ID gerada no Cloudflare R2
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Secret Access Key gerada no Cloudflare R2
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Valida se todas as configurações necessárias estão preenchidas
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new InvalidOperationException("R2Storage:Endpoint não está configurado");

        if (string.IsNullOrWhiteSpace(BucketName))
            throw new InvalidOperationException("R2Storage:BucketName não está configurado");

        if (string.IsNullOrWhiteSpace(PublicUrl))
            throw new InvalidOperationException("R2Storage:PublicUrl não está configurado");

        if (string.IsNullOrWhiteSpace(AccessKeyId))
            throw new InvalidOperationException("R2Storage:AccessKeyId não está configurado");

        if (string.IsNullOrWhiteSpace(SecretAccessKey))
            throw new InvalidOperationException("R2Storage:SecretAccessKey não está configurado");
    }
}
