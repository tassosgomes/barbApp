namespace BarbApp.Application.DTOs;

public record CriarAgendamentoInput(
    Guid BarbeiroId,
    List<Guid> ServicosIds,
    DateTime DataHora
);