namespace BarbApp.Infrastructure.Configuration;

/// <summary>
/// Configurações do servidor SMTP para envio de e-mails.
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// Host do servidor SMTP.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Porta do servidor SMTP.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Indica se deve usar SSL/TLS para conexão segura.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Nome de usuário para autenticação SMTP (opcional).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha para autenticação SMTP (opcional).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Endereço de e-mail do remetente.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nome amigável do remetente.
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o servidor SMTP requer autenticação.
    /// Retorna true se Username não estiver vazio.
    /// </summary>
    public bool RequiresAuthentication => !string.IsNullOrWhiteSpace(Username);
}
