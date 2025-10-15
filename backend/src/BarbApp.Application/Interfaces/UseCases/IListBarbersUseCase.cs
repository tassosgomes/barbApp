using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IListBarbersUseCase
{
    Task<PaginatedBarbersOutput> ExecuteAsync(
        bool? isActive,
        string? searchName,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}