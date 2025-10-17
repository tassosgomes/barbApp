namespace BarbApp.Application.Interfaces;

/// <summary>
/// Interface para serviço de envio de e-mails transacionais.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia um e-mail de forma assíncrona.
    /// Implementa retry automático em caso de falhas temporárias.
    /// </summary>
    /// <param name="message">Mensagem de e-mail a ser enviada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task que completa quando o e-mail é enviado com sucesso.</returns>
    /// <exception cref="InvalidOperationException">Lançada após falhar 3 tentativas de envio.</exception>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Representa uma mensagem de e-mail com conteúdo HTML e texto simples.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Endereço de e-mail do destinatário.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Assunto do e-mail.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Corpo do e-mail em formato HTML.
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Corpo do e-mail em formato texto simples (fallback para clientes que não suportam HTML).
    /// </summary>
    public string TextBody { get; set; } = string.Empty;
}
