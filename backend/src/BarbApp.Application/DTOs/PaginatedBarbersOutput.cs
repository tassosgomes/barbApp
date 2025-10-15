namespace BarbApp.Application.DTOs;

public record PaginatedBarbersOutput(
    List<BarberOutput> Barbers,
    int TotalCount,
    int Page,
    int PageSize
);