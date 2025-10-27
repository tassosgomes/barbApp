// BarbApp.Application/Interfaces/UseCases/ILoginClienteUseCase.cs
using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ILoginClienteUseCase
{
    Task<LoginClienteOutput> Handle(LoginClienteInput input, CancellationToken cancellationToken = default);
}