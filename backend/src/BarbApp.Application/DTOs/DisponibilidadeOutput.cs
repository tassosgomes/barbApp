namespace BarbApp.Application.DTOs;

public record DisponibilidadeOutput(
    BarbeiroDto Barbeiro,
    List<DiaDisponivel> DiasDisponiveis
);

public record DiaDisponivel(
    DateTime Data,
    List<string> HorariosDisponiveis
);