using BarbApp.Application.Interfaces;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

/// <summary>
/// Use case para redefinir a senha de um barbeiro
/// Gera uma nova senha segura, atualiza no banco e envia por email
/// </summary>
public class ResetBarberPasswordUseCase : IResetBarberPasswordUseCase
{
    private readonly IBarberRepository _barberRepository;
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetBarberPasswordUseCase> _logger;

    public ResetBarberPasswordUseCase(
        IBarberRepository barberRepository,
        IBarbershopRepository barbershopRepository,
        ITenantContext tenantContext,
        IPasswordGenerator passwordGenerator,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<ResetBarberPasswordUseCase> logger)
    {
        _barberRepository = barberRepository;
        _barbershopRepository = barbershopRepository;
        _tenantContext = tenantContext;
        _passwordGenerator = passwordGenerator;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid barberId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting password reset for barber {BarberId}", barberId);

        // Obter BarbeariaId do contexto tenant do Admin Barbearia autenticado
        var barbeariaId = _tenantContext.BarbeariaId;
        if (barbeariaId == null)
        {
            _logger.LogError("Failed to reset barber password: Tenant context not defined");
            throw new BarbApp.Domain.Exceptions.UnauthorizedAccessException("Contexto de barbearia não definido");
        }

        _logger.LogInformation("Admin Barbearia from barbershop {BarbeariaId} requesting password reset for barber {BarberId}", 
            barbeariaId, barberId);

        // Buscar barbeiro
        var barber = await _barberRepository.GetByIdAsync(barberId, cancellationToken);
        if (barber == null)
        {
            _logger.LogWarning("Barber {BarberId} not found for password reset", barberId);
            throw new NotFoundException($"Barbeiro com ID '{barberId}' não encontrado");
        }

        // Validar isolamento multi-tenant: barbeiro deve pertencer à mesma barbearia do admin autenticado
        if (barber.BarbeariaId != barbeariaId.Value)
        {
            _logger.LogWarning(
                "Forbidden: Admin from barbershop {AdminBarbeariaId} tried to reset password of barber {BarberId} from barbershop {BarberBarbeariaId}",
                barbeariaId, barberId, barber.BarbeariaId);
            throw new ForbiddenException("Você não tem permissão para redefinir a senha deste barbeiro");
        }

        // Buscar barbearia para incluir informações no email
        var barbershop = await _barbershopRepository.GetByIdAsync(barbeariaId.Value, cancellationToken);
        if (barbershop == null)
        {
            _logger.LogError("Barbershop {BarbeariaId} not found when resetting password for barber {BarberId}", 
                barbeariaId, barberId);
            throw new NotFoundException($"Barbearia não encontrada");
        }

        // Gerar nova senha segura
        var newPassword = _passwordGenerator.Generate();
        _logger.LogInformation("Generated new password for barber {BarberId} ({BarberName})", 
            barberId, barber.Name);

        // Atualizar senha do barbeiro
        var newPasswordHash = _passwordHasher.Hash(newPassword);
        barber.ChangePassword(newPasswordHash);
        await _barberRepository.UpdateAsync(barber, cancellationToken);
        _logger.LogInformation("Password hash updated for barber {BarberId}", barberId);

        try
        {
            // Enviar e-mail com nova senha
            var emailMessage = new EmailMessage
            {
                To = barber.Email,
                Subject = "BarbApp - Sua senha foi redefinida",
                HtmlBody = BuildResetPasswordEmailHtml(barber.Name, barbershop.Name, barber.Email, newPassword),
                TextBody = BuildResetPasswordEmailText(barber.Name, barbershop.Name, barber.Email, newPassword)
            };

            await _emailService.SendAsync(emailMessage, cancellationToken);
            _logger.LogInformation(
                "Password reset email sent successfully to {Email} for barber {BarberId} of barbershop {BarbeariaId}", 
                barber.Email, barberId, barbeariaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send password reset email to {Email} for barber {BarberId}. Rolling back transaction.", 
                barber.Email, barberId);
            throw new InvalidOperationException(
                $"Falha ao enviar e-mail com a nova senha. Por favor, tente novamente ou contate o suporte.", ex);
        }

        // Commit transaction apenas após envio bem-sucedido do e-mail
        await _unitOfWork.Commit(cancellationToken);
        _logger.LogInformation(
            "Password reset completed successfully for barber {BarberId} ({BarberName}) of barbershop {BarbeariaId}", 
            barberId, barber.Name, barbeariaId);
    }

    private static string BuildResetPasswordEmailHtml(string barberName, string barbershopName, string email, string password)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Senha Redefinida - BarbApp</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background-color: #f8f9fa; border-radius: 10px; padding: 30px; margin-bottom: 20px;"">
        <h1 style=""color: #2c3e50; margin-bottom: 10px;"">Sua senha foi redefinida 🔑</h1>
        <p style=""font-size: 16px; margin-bottom: 20px;"">
            Olá, <strong>{barberName}</strong>! O administrador da barbearia <strong>{barbershopName}</strong> redefiniu sua senha de acesso.
        </p>
    </div>

    <div style=""background-color: #fff; border: 1px solid #dee2e6; border-radius: 10px; padding: 30px; margin-bottom: 20px;"">
        <h2 style=""color: #2c3e50; margin-bottom: 15px;"">Suas Novas Credenciais</h2>
        <p style=""margin-bottom: 15px;"">Use as credenciais abaixo para acessar o aplicativo:</p>
        
        <div style=""background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 5px 0;""><strong>E-mail:</strong> {email}</p>
            <p style=""margin: 5px 0;""><strong>Nova Senha:</strong> <code style=""background-color: #e9ecef; padding: 5px 10px; border-radius: 3px; font-size: 14px; font-family: 'Courier New', monospace;"">{password}</code></p>
        </div>

        <div style=""background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 0; color: #856404;"">
                <strong>⚠️ Importante para sua segurança:</strong><br>
                • Sua senha anterior não é mais válida<br>
                • Guarde esta senha em local seguro<br>
                • Recomendamos alterá-la após o primeiro acesso<br>
                • Não compartilhe sua senha com ninguém
            </p>
        </div>

        <div style=""background-color: #d1ecf1; border-left: 4px solid #17a2b8; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 0; color: #0c5460;"">
                <strong>ℹ️ Não solicitou esta alteração?</strong><br>
                Entre em contato imediatamente com o administrador da sua barbearia ou com o suporte do BarbApp.
            </p>
        </div>
    </div>

    <div style=""text-align: center; color: #6c757d; font-size: 14px; padding-top: 20px; border-top: 1px solid #dee2e6;"">
        <p>Caso tenha alguma dúvida, entre em contato com o administrador da sua barbearia.</p>
        <p style=""margin: 5px 0;"">BarbApp - Gestão de Barbearias</p>
    </div>
</body>
</html>";
    }

    private static string BuildResetPasswordEmailText(string barberName, string barbershopName, string email, string password)
    {
        return $@"Sua senha foi redefinida - BarbApp

Olá, {barberName}! O administrador da barbearia ""{barbershopName}"" redefiniu sua senha de acesso.

=== SUAS NOVAS CREDENCIAIS ===

E-mail: {email}
Nova Senha: {password}

⚠️ IMPORTANTE PARA SUA SEGURANÇA:
• Sua senha anterior não é mais válida
• Guarde esta senha em local seguro
• Recomendamos alterá-la após o primeiro acesso
• Não compartilhe sua senha com ninguém

ℹ️ NÃO SOLICITOU ESTA ALTERAÇÃO?
Entre em contato imediatamente com o administrador da sua barbearia ou com o suporte do BarbApp.

Caso tenha alguma dúvida, entre em contato com o administrador da sua barbearia.

BarbApp - Gestão de Barbearias";
    }
}
