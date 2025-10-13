namespace BarbApp.Application.Interfaces.UseCases;

public interface IDeleteBarbershopUseCase
{
    Task ExecuteAsync(Guid id, CancellationToken cancellationToken);
}