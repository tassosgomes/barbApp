// BarbApp.Application/UseCases/ListarServicosUseCase.cs
using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListarServicosUseCase : IListarServicosUseCase
{
    private readonly IServicosRepository _servicosRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListarServicosUseCase> _logger;

    public ListarServicosUseCase(
        IServicosRepository servicosRepository,
        IMapper mapper,
        ILogger<ListarServicosUseCase> logger)
    {
        _servicosRepository = servicosRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<ServicoDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listando serviços ativos da barbearia {BarbeariaId}", barbeariaId);

        var servicos = await _servicosRepository.GetAtivosAsync(
            barbeariaId,
            cancellationToken);

        _logger.LogInformation("Encontrados {Count} serviços ativos", servicos.Count);

        return _mapper.Map<List<ServicoDto>>(servicos);
    }
}