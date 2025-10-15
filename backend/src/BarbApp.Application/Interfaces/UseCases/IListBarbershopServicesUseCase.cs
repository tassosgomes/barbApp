using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IListBarbershopServicesUseCase
{
    Task<PaginatedBarbershopServicesOutput> ExecuteAsync(
        bool? isActive,
        string? searchName,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}