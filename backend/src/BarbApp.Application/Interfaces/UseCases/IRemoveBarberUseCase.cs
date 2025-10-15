namespace BarbApp.Application.Interfaces.UseCases;

public interface IRemoveBarberUseCase
{
    Task ExecuteAsync(Guid barberId, CancellationToken cancellationToken);
}