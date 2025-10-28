// BarbApp.Application/Interfaces/UseCases/ICancelarAgendamentoUseCase.cs

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICancelarAgendamentoUseCase
{
    Task Handle(Guid clienteId, Guid agendamentoId, CancellationToken cancellationToken = default);
}