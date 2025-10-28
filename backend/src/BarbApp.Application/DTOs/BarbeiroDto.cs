namespace BarbApp.Application.DTOs;

public record BarbeiroDto(
    Guid Id,
    string Nome,
    string? Foto,
    List<string> Especialidades
)
{
    public BarbeiroDto() : this(Guid.Empty, string.Empty, null, new List<string>()) { }
}