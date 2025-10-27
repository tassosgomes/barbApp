// BarbApp.Application/Interfaces/UseCases/ICadastrarClienteUseCase.cs
using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface ICadastrarClienteUseCase
{
    Task<CadastroClienteOutput> Handle(CadastrarClienteInput input, CancellationToken cancellationToken = default);
}