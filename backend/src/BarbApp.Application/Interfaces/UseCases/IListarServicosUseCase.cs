using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IListarServicosUseCase
{
    Task<List<ServicoDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default);
}