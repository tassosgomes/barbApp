// BarbApp.Application/Interfaces/UseCases/ICriarAgendamentoUseCase.cs
using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICriarAgendamentoUseCase
{
    Task<AgendamentoOutput> Handle(Guid clienteId, Guid barbeariaId, CriarAgendamentoInput input, CancellationToken cancellationToken = default);
}