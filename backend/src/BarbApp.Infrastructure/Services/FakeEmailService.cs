using BarbApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Implementação fake de serviço de envio de e-mails para desenvolvimento.
/// Não envia e-mails reais, apenas loga as mensagens.
/// Útil para testes locais sem necessidade de configurar SMTP.
/// </summary>
public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            """
            ========================================
            📧 FAKE EMAIL SERVICE - Email NÃO enviado
            ========================================
            Para: {To}
            Assunto: {Subject}
            ----------------------------------------
            Corpo HTML:
            {HtmlBody}
            ----------------------------------------
            Corpo Texto:
            {TextBody}
            ========================================
            """,
            message.To,
            message.Subject,
            message.HtmlBody,
            message.TextBody
        );

        // Simular sucesso imediato
        return Task.CompletedTask;
    }
}
