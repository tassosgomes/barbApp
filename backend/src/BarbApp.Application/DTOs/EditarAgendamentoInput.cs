namespace BarbApp.Application.DTOs;

public record EditarAgendamentoInput(
    Guid AgendamentoId,
    Guid? BarbeiroId,
    List<Guid>? ServicosIds,
    DateTime? DataHora
);