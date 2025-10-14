namespace BarbApp.Application.Interfaces.UseCases;

public interface IReactivateBarbershopUseCase
{
    Task ExecuteAsync(Guid id, CancellationToken cancellationToken);
}