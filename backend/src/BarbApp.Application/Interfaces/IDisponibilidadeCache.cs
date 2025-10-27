namespace BarbApp.Application.Interfaces;

public interface IDisponibilidadeCache
{
    Task<DTOs.DisponibilidadeOutput?> GetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task SetAsync(Guid barbeiroId, DateTime dataInicio, DateTime dataFim, DTOs.DisponibilidadeOutput disponibilidade, CancellationToken cancellationToken = default);
}