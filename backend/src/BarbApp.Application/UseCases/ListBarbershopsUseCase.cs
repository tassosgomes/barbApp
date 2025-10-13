using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Common;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListBarbershopsUseCase : IListBarbershopsUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly ILogger<ListBarbershopsUseCase> _logger;

    public ListBarbershopsUseCase(IBarbershopRepository barbershopRepository, ILogger<ListBarbershopsUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _logger = logger;
    }

    public async Task<PaginatedBarbershopsOutput> ExecuteAsync(
        int page,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        string? sortBy,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing barbershops with page {Page}, pageSize {PageSize}, searchTerm {SearchTerm}, isActive {IsActive}, sortBy {SortBy}",
            page, pageSize, searchTerm, isActive, sortBy);

        var result = await _barbershopRepository.ListAsync(page, pageSize, searchTerm, isActive, sortBy, cancellationToken);

        var items = result.Items.Select(b => new BarbershopOutput(
            b.Id,
            b.Name,
            b.Document.Value,
            b.Phone,
            b.OwnerName,
            b.Email,
            b.Code.Value,
            b.IsActive,
            new AddressOutput(
                b.Address.ZipCode,
                b.Address.Street,
                b.Address.Number,
                b.Address.Complement,
                b.Address.Neighborhood,
                b.Address.City,
                b.Address.State),
            b.CreatedAt,
            b.UpdatedAt)).ToList();

        _logger.LogInformation("Retrieved {ItemCount} barbershops out of {TotalCount} total", items.Count, result.TotalCount);

        return new PaginatedBarbershopsOutput(items, result.TotalCount, result.Page, result.PageSize);
    }
}