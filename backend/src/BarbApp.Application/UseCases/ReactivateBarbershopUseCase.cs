using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class ReactivateBarbershopUseCase : IReactivateBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReactivateBarbershopUseCase> _logger;

    public ReactivateBarbershopUseCase(IBarbershopRepository barbershopRepository, IUnitOfWork unitOfWork, ILogger<ReactivateBarbershopUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting reactivation of barbershop with ID {BarbershopId}", id);

        var barbershop = await _barbershopRepository.GetByIdAsync(id, cancellationToken);
        if (barbershop == null)
        {
            _logger.LogWarning("Barbershop with ID {BarbershopId} not found for reactivation", id);
            throw new BarbershopNotFoundException("Barbearia não encontrada");
        }

        barbershop.Activate();
        await _barbershopRepository.UpdateAsync(barbershop, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Barbershop with ID {BarbershopId} reactivated successfully", id);
    }
}