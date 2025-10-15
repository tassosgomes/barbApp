namespace BarbApp.Application.DTOs;

public record PaginatedBarbershopServicesOutput(
    List<BarbershopServiceOutput> Services,
    int TotalCount,
    int Page,
    int PageSize
);