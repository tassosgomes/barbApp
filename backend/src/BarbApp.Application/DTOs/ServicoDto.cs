namespace BarbApp.Application.DTOs;

public record ServicoDto(
    Guid Id,
    string Nome,
    string Descricao,
    int DuracaoMinutos,
    decimal? Preco
);