namespace BarbApp.Application.Interfaces;

/// <summary>
/// Interface para o caso de uso de reenvio de credenciais do Admin Barbearia.
/// Gera uma nova senha e envia por e-mail.
/// </summary>
public interface IResendCredentialsUseCase
{
    /// <summary>
    /// Reenvia as credenciais de acesso do Admin Barbearia.
    /// Gera uma nova senha segura, atualiza no banco e envia por e-mail.
    /// </summary>
    /// <param name="barbershopId">ID da barbearia cujo admin terá as credenciais reenviadas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <exception cref="NotFoundException">Quando a barbearia não é encontrada</exception>
    /// <exception cref="ValidationException">Quando o Admin Barbearia não é encontrado ou outros erros de validação</exception>
    Task ExecuteAsync(Guid barbershopId, CancellationToken cancellationToken = default);
}
