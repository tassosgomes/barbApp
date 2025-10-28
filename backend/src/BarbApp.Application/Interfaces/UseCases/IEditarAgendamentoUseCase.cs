// BarbApp.Application/Interfaces/UseCases/IEditarAgendamentoUseCase.cs
using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IEditarAgendamentoUseCase
{
    Task<AgendamentoOutput> Handle(Guid clienteId, Guid barbeariaId, EditarAgendamentoInput input, CancellationToken cancellationToken = default);
}