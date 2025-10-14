using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class DeactivateBarbershopUseCase : IDeactivateBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateBarbershopUseCase> _logger;

    public DeactivateBarbershopUseCase(IBarbershopRepository barbershopRepository, IUnitOfWork unitOfWork, ILogger<DeactivateBarbershopUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting deactivation of barbershop with ID {BarbershopId}", id);

        var barbershop = await _barbershopRepository.GetByIdAsync(id, cancellationToken);
        if (barbershop == null)
        {
            _logger.LogWarning("Barbershop with ID {BarbershopId} not found for deactivation", id);
            throw new BarbershopNotFoundException("Barbearia não encontrada");
        }

        barbershop.Deactivate();
        await _barbershopRepository.UpdateAsync(barbershop, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Barbershop with ID {BarbershopId} deactivated successfully", id);
    }
}