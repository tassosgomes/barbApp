using BarbApp.Application.Interfaces;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ResendCredentialsUseCase : IResendCredentialsUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IAdminBarbeariaUserRepository _adminBarbeariaUserRepository;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResendCredentialsUseCase> _logger;

    public ResendCredentialsUseCase(
        IBarbershopRepository barbershopRepository,
        IAdminBarbeariaUserRepository adminBarbeariaUserRepository,
        IPasswordGenerator passwordGenerator,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<ResendCredentialsUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _adminBarbeariaUserRepository = adminBarbeariaUserRepository;
        _passwordGenerator = passwordGenerator;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid barbershopId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting credential resend for barbershop {BarbershopId}", barbershopId);

        // Buscar barbearia
        var barbershop = await _barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);
        if (barbershop == null)
        {
            _logger.LogWarning("Barbershop {BarbershopId} not found for credential resend", barbershopId);
            throw new NotFoundException($"Barbearia com ID '{barbershopId}' não encontrada");
        }

        // Buscar Admin Barbearia vinculado
        var adminUser = await _adminBarbeariaUserRepository.GetByBarbershopIdAsync(barbershopId, cancellationToken);
        if (adminUser == null)
        {
            _logger.LogWarning("Admin Barbearia not found for barbershop {BarbershopId}", barbershopId);
            throw new ValidationException($"Administrador da barbearia '{barbershop.Name}' não encontrado");
        }

        // Gerar nova senha segura
        var newPassword = _passwordGenerator.Generate();
        _logger.LogInformation("Generated new password for Admin Barbearia {AdminId} of barbershop {BarbershopId}", adminUser.Id, barbershopId);

        // Atualizar password_hash
        var newPasswordHash = _passwordHasher.Hash(newPassword);
        adminUser.UpdatePassword(newPasswordHash);
        await _adminBarbeariaUserRepository.UpdateAsync(adminUser, cancellationToken);
        _logger.LogInformation("Password updated for Admin Barbearia {AdminId}", adminUser.Id);

        try
        {
            // Enviar e-mail com nova senha
            var emailMessage = new EmailMessage
            {
                To = adminUser.Email,
                Subject = "BarbApp - Suas novas credenciais de acesso",
                HtmlBody = BuildResendEmailHtml(barbershop.Name, adminUser.Email, newPassword),
                TextBody = BuildResendEmailText(barbershop.Name, adminUser.Email, newPassword)
            };

            await _emailService.SendAsync(emailMessage, cancellationToken);
            _logger.LogInformation("Credentials resend email sent successfully to {Email} for barbershop {BarbershopId}", adminUser.Email, barbershopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send credentials resend email to {Email} for barbershop {BarbershopId}. Rolling back transaction.", adminUser.Email, barbershopId);
            throw new InvalidOperationException($"Falha ao enviar e-mail com as novas credenciais. Por favor, tente novamente ou contate o suporte.", ex);
        }

        // Commit transaction apenas após envio bem-sucedido do e-mail
        await _unitOfWork.Commit(cancellationToken);
        _logger.LogInformation("Credentials resent successfully for barbershop {BarbershopId}", barbershopId);
    }

    private static string BuildResendEmailHtml(string barbershopName, string email, string password)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Novas Credenciais - BarbApp</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background-color: #f8f9fa; border-radius: 10px; padding: 30px; margin-bottom: 20px;"">
        <h1 style=""color: #2c3e50; margin-bottom: 10px;"">Novas Credenciais de Acesso 🔑</h1>
        <p style=""font-size: 16px; margin-bottom: 20px;"">
            Olá! Conforme solicitado, geramos novas credenciais de acesso para a barbearia <strong>{barbershopName}</strong>.
        </p>
    </div>

    <div style=""background-color: #fff; border: 1px solid #dee2e6; border-radius: 10px; padding: 30px; margin-bottom: 20px;"">
        <h2 style=""color: #2c3e50; margin-bottom: 15px;"">Suas Novas Credenciais</h2>
        <p style=""margin-bottom: 15px;"">Use as credenciais abaixo para acessar o painel administrativo:</p>
        
        <div style=""background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 5px 0;""><strong>E-mail:</strong> {email}</p>
            <p style=""margin: 5px 0;""><strong>Nova Senha:</strong> <code style=""background-color: #e9ecef; padding: 5px 10px; border-radius: 3px; font-size: 14px; font-family: 'Courier New', monospace;"">{password}</code></p>
        </div>

        <div style=""background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 0; color: #856404;"">
                <strong>⚠️ Importante:</strong> Sua senha anterior não é mais válida. Guarde sua nova senha em local seguro e recomendamos alterá-la após o primeiro acesso.
            </p>
        </div>

        <div style=""background-color: #d1ecf1; border-left: 4px solid #17a2b8; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 0; color: #0c5460;"">
                <strong>ℹ️ Caso não tenha solicitado esta alteração</strong>, entre em contato imediatamente com o suporte.
            </p>
        </div>
    </div>

    <div style=""text-align: center; color: #6c757d; font-size: 14px; padding-top: 20px; border-top: 1px solid #dee2e6;"">
        <p>Caso tenha alguma dúvida, entre em contato com nosso suporte.</p>
        <p style=""margin: 5px 0;"">BarbApp - Gestão de Barbearias</p>
    </div>
</body>
</html>";
    }

    private static string BuildResendEmailText(string barbershopName, string email, string password)
    {
        return $@"Novas Credenciais de Acesso - BarbApp

Olá! Conforme solicitado, geramos novas credenciais de acesso para a barbearia ""{barbershopName}"".

=== SUAS NOVAS CREDENCIAIS ===

E-mail: {email}
Nova Senha: {password}

⚠️ IMPORTANTE: Sua senha anterior não é mais válida. Guarde sua nova senha em local seguro e recomendamos alterá-la após o primeiro acesso.

ℹ️ Caso não tenha solicitado esta alteração, entre em contato imediatamente com o suporte.

Caso tenha alguma dúvida, entre em contato com nosso suporte.

BarbApp - Gestão de Barbearias";
    }
}
