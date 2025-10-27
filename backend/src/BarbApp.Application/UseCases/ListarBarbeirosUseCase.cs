// BarbApp.Application/UseCases/ListarBarbeirosUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListarBarbeirosUseCase : IListarBarbeirosUseCase
{
    private readonly IBarberRepository _barbeirosRepository;
    private readonly ILogger<ListarBarbeirosUseCase> _logger;

    public ListarBarbeirosUseCase(
        IBarberRepository barbeirosRepository,
        ILogger<ListarBarbeirosUseCase> logger)
    {
        _barbeirosRepository = barbeirosRepository;
        _logger = logger;
    }

    public async Task<List<BarbeiroDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listando barbeiros ativos da barbearia {BarbeariaId}", barbeariaId);

        var barbeiros = await _barbeirosRepository.ListAsync(
            barbeariaId,
            isActive: true,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Encontrados {Count} barbeiros ativos", barbeiros.Count);

        return barbeiros.Select(b => new BarbeiroDto(
            b.Id,
            b.Name,
            null, // Foto não implementada
            new List<string>() // Especialidades não implementadas
        )).ToList();
    }
}