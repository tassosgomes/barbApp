// BarbApp.Application/UseCases/ListarBarbeirosUseCase.cs
using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListarBarbeirosUseCase : IListarBarbeirosUseCase
{
    private readonly IBarbeirosRepository _barbeirosRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListarBarbeirosUseCase> _logger;

    public ListarBarbeirosUseCase(
        IBarbeirosRepository barbeirosRepository,
        IMapper mapper,
        ILogger<ListarBarbeirosUseCase> logger)
    {
        _barbeirosRepository = barbeirosRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<BarbeiroDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listando barbeiros ativos da barbearia {BarbeariaId}", barbeariaId);

        var barbeiros = await _barbeirosRepository.GetAtivosAsync(
            barbeariaId,
            cancellationToken);

        _logger.LogInformation("Encontrados {Count} barbeiros ativos", barbeiros.Count);

        return _mapper.Map<List<BarbeiroDto>>(barbeiros);
    }
}