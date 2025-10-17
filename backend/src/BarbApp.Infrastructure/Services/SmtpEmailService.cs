using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using BarbApp.Application.Interfaces;
using BarbApp.Infrastructure.Configuration;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Implementação de serviço de envio de e-mails usando SMTP via MailKit.
/// Suporta retry automático com backoff exponencial em caso de falhas temporárias.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Envia um e-mail com retry automático (até 3 tentativas).
    /// Backoff exponencial: 1s, 2s, 4s entre tentativas.
    /// </summary>
    /// <param name="message">Mensagem a ser enviada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <exception cref="InvalidOperationException">Lançada após 3 tentativas falhadas.</exception>
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        var retryDelays = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) };

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                using var client = new SmtpClient();
                
                // Conectar ao servidor SMTP
                await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, cancellationToken);

                // Autenticar apenas se credenciais forem fornecidas
                if (_settings.RequiresAuthentication)
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
                    _logger.LogDebug("SMTP authentication successful for {Host}:{Port}", _settings.Host, _settings.Port);
                }
                else
                {
                    _logger.LogDebug("SMTP connection without authentication (dev mode) to {Host}:{Port}", _settings.Host, _settings.Port);
                }

                // Construir mensagem MIME
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                mimeMessage.To.Add(MailboxAddress.Parse(message.To));
                mimeMessage.Subject = message.Subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message.HtmlBody,
                    TextBody = message.TextBody
                };
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                // Enviar e-mail
                await client.SendAsync(mimeMessage, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent successfully to {Recipient} with subject '{Subject}'", 
                    message.To, message.Subject);
                
                return; // Sucesso, sair do loop
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, 
                    "Failed to send email to {Recipient} on attempt {Attempt}/{MaxRetries}", 
                    message.To, attempt + 1, maxRetries);

                // Se foi a última tentativa, lançar exceção
                if (attempt == maxRetries - 1)
                {
                    _logger.LogError(ex, 
                        "Failed to send email to {Recipient} after {MaxRetries} attempts", 
                        message.To, maxRetries);
                    
                    throw new InvalidOperationException(
                        $"Failed to send email after {maxRetries} attempts.", ex);
                }

                // Aguardar antes da próxima tentativa (backoff exponencial)
                await Task.Delay(retryDelays[attempt], cancellationToken);
            }
        }
    }
}
