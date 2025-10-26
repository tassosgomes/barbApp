// BarbApp.Domain/Interfaces/Repositories/IAgendamentoRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;

namespace BarbApp.Domain.Interfaces.Repositories;

public interface IAgendamentoRepository
{
    Task<List<Agendamento>> GetByBarbeiroAndDateRangeAsync(
        Guid barbeiroId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    Task<List<Agendamento>> GetByClienteAsync(
        Guid clienteId,
        StatusAgendamento? status = null,
        CancellationToken cancellationToken = default);

    Task<Agendamento?> GetByIdAsync(Guid agendamentoId, CancellationToken cancellationToken = default);

    Task AddAsync(Agendamento agendamento, CancellationToken cancellationToken = default);

    Task<bool> ExisteConflito(
        Guid barbeiroId,
        DateTime dataHora,
        int duracaoMinutos,
        Guid? agendamentoIdParaIgnorar = null,
        CancellationToken cancellationToken = default);
}