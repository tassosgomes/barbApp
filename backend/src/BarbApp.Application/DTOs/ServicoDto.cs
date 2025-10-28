namespace BarbApp.Application.DTOs;

public record ServicoDto(
    Guid Id,
    string Nome,
    string Descricao,
    int DuracaoMinutos,
    decimal? Preco
)
{
    public ServicoDto() : this(Guid.Empty, string.Empty, string.Empty, 0, null) { }
}