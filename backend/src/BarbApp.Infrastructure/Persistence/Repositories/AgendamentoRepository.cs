// BarbApp.Infrastructure/Persistence/Repositories/AgendamentoRepository.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Persistence.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly BarbAppDbContext _context;

    public AgendamentoRepository(BarbAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Agendamento>> GetByBarbeiroAndDateRangeAsync(
        Guid barbeiroId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        return await _context.Agendamentos
            .Where(a => a.BarbeiroId == barbeiroId &&
                       a.DataHora >= dataInicio &&
                       a.DataHora < dataFim)
            .OrderBy(a => a.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Agendamento>> GetByClienteAsync(
        Guid clienteId,
        StatusAgendamento? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Agendamentos
            .Where(a => a.ClienteId == clienteId);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query
            .OrderByDescending(a => a.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<Agendamento?> GetByIdAsync(Guid agendamentoId, CancellationToken cancellationToken = default)
    {
        return await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == agendamentoId, cancellationToken);
    }

    public async Task AddAsync(Agendamento agendamento, CancellationToken cancellationToken = default)
    {
        await _context.Agendamentos.AddAsync(agendamento, cancellationToken);
    }

    public async Task UpdateAsync(Agendamento agendamento, CancellationToken cancellationToken = default)
    {
        _context.Agendamentos.Update(agendamento);
    }

    public async Task<bool> ExisteConflito(
        Guid barbeiroId,
        DateTime dataHora,
        int duracaoMinutos,
        Guid? agendamentoIdParaIgnorar = null,
        CancellationToken cancellationToken = default)
    {
        var dataHoraFim = dataHora.AddMinutes(duracaoMinutos);

        var query = _context.Agendamentos
            .Where(a => a.BarbeiroId == barbeiroId &&
                       a.Status != StatusAgendamento.Cancelado &&
                       a.Status != StatusAgendamento.Concluido &&
                       ((a.DataHora < dataHoraFim && a.DataHora.AddMinutes(a.DuracaoMinutos) > dataHora)));

        if (agendamentoIdParaIgnorar.HasValue)
        {
            query = query.Where(a => a.Id != agendamentoIdParaIgnorar.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}