namespace BarbApp.Application.Interfaces;

/// <summary>
/// Use case para redefinir senha de um barbeiro
/// </summary>
public interface IResetBarberPasswordUseCase
{
    /// <summary>
    /// Gera uma nova senha segura para o barbeiro e envia por email
    /// </summary>
    /// <param name="barberId">ID do barbeiro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Barbeiro não encontrado</exception>
    /// <exception cref="Domain.Exceptions.ForbiddenException">Barbeiro não pertence à barbearia do usuário autenticado</exception>
    Task ExecuteAsync(Guid barberId, CancellationToken cancellationToken = default);
}
