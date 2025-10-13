namespace BarbApp.Application.DTOs;

public record PaginatedBarbershopsOutput(
    List<BarbershopOutput> Items,
    int TotalCount,
    int Page,
    int PageSize
);