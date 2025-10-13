using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.UseCases;

public class DeleteBarbershopUseCase : IDeleteBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBarbershopUseCase(IBarbershopRepository barbershopRepository, IUnitOfWork unitOfWork)
    {
        _barbershopRepository = barbershopRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var barbershop = await _barbershopRepository.GetByIdAsync(id, cancellationToken);
        if (barbershop == null)
        {
            throw new BarbershopNotFoundException("Barbearia não encontrada");
        }

        await _barbershopRepository.DeleteAsync(barbershop, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
    }
}