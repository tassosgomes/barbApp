using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Common;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class ListBarbershopsUseCase : IListBarbershopsUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;

    public ListBarbershopsUseCase(IBarbershopRepository barbershopRepository)
    {
        _barbershopRepository = barbershopRepository;
    }

    public async Task<PaginatedBarbershopsOutput> ExecuteAsync(
        int page,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        string? sortBy,
        CancellationToken cancellationToken)
    {
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

        return new PaginatedBarbershopsOutput(items, result.TotalCount, result.Page, result.PageSize);
    }
}