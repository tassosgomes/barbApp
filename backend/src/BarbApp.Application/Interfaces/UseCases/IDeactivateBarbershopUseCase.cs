namespace BarbApp.Application.Interfaces.UseCases;

public interface IDeactivateBarbershopUseCase
{
    Task ExecuteAsync(Guid id, CancellationToken cancellationToken);
}