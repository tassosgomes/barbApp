// BarbApp.Application/UseCases/ListarServicosUseCase.cs
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ListarServicosUseCase : IListarServicosUseCase
{
    private readonly IBarbershopServiceRepository _servicosRepository;
    private readonly ILogger<ListarServicosUseCase> _logger;

    public ListarServicosUseCase(
        IBarbershopServiceRepository servicosRepository,
        ILogger<ListarServicosUseCase> logger)
    {
        _servicosRepository = servicosRepository;
        _logger = logger;
    }

    public async Task<List<ServicoDto>> Handle(Guid barbeariaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listando serviços ativos da barbearia {BarbeariaId}", barbeariaId);

        var servicos = await _servicosRepository.ListAsync(
            barbeariaId,
            isActive: true,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Encontrados {Count} serviços ativos", servicos.Count);

        return servicos.Select(s => new ServicoDto(
            s.Id,
            s.Name,
            s.Description ?? string.Empty,
            s.DurationMinutes,
            s.Price
        )).ToList();
    }
}