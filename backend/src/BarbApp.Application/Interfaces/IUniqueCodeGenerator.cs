namespace BarbApp.Application.Interfaces;

public interface IUniqueCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}