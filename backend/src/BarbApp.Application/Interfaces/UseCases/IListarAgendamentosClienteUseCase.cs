// BarbApp.Application/Interfaces/UseCases/IListarAgendamentosClienteUseCase.cs
using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IListarAgendamentosClienteUseCase
{
    Task<List<AgendamentoOutput>> Handle(Guid clienteId, string filtro, CancellationToken cancellationToken = default);
}