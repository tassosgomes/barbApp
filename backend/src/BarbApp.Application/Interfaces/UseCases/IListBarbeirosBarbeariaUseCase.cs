using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IListBarbeirosBarbeariaUseCase
{
    Task<IEnumerable<BarberInfo>> ExecuteAsync(CancellationToken cancellationToken = default);
}

