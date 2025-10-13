using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IListBarbershopsUseCase
{
    Task<PaginatedBarbershopsOutput> ExecuteAsync(int page, int pageSize, string? searchTerm, bool? isActive, string? sortBy, CancellationToken cancellationToken);
}