using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IConsultarDisponibilidadeUseCase
{
    Task<DisponibilidadeOutput> Handle(
        Guid barbeiroId,
        DateTime dataInicio,
        DateTime dataFim,
        int duracaoServicosMinutos,
        CancellationToken cancellationToken = default);
}