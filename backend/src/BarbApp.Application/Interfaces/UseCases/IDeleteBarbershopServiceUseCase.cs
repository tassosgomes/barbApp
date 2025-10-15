namespace BarbApp.Application.Interfaces.UseCases;

public interface IDeleteBarbershopServiceUseCase
{
    Task ExecuteAsync(Guid serviceId, CancellationToken cancellationToken);
}