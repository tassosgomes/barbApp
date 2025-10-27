using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IListarBarbeirosUseCase
{
    Task<List<BarbeiroDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default);
}