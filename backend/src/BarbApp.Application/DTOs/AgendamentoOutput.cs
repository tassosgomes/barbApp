namespace BarbApp.Application.DTOs;

public record AgendamentoOutput(
    Guid Id,
    BarbeiroDto Barbeiro,
    List<ServicoDto> Servicos,
    DateTime DataHora,
    int DuracaoTotal,
    string Status
);