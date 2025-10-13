using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class DeleteBarbershopUseCase : IDeleteBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBarbershopUseCase> _logger;

    public DeleteBarbershopUseCase(IBarbershopRepository barbershopRepository, IUnitOfWork unitOfWork, ILogger<DeleteBarbershopUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting deletion of barbershop with ID {BarbershopId}", id);

        var barbershop = await _barbershopRepository.GetByIdAsync(id, cancellationToken);
        if (barbershop == null)
        {
            _logger.LogWarning("Barbershop with ID {BarbershopId} not found for deletion", id);
            throw new BarbershopNotFoundException("Barbearia não encontrada");
        }

        await _barbershopRepository.DeleteAsync(barbershop, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Barbershop with ID {BarbershopId} deleted successfully", id);
    }
}