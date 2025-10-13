using BarbApp.Application.Interfaces;
using BarbApp.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace BarbApp.Infrastructure.Services;

public class UniqueCodeGenerator : IUniqueCodeGenerator
{
    private readonly IBarbershopRepository _repository;
    private readonly ILogger<UniqueCodeGenerator> _logger;
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Sem O, I, 0, 1
    private const int CodeLength = 8;
    private const int MaxRetries = 10;

    public UniqueCodeGenerator(IBarbershopRepository repository, ILogger<UniqueCodeGenerator> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            var code = GenerateRandomCode();

            var existing = await _repository.GetByCodeAsync(code, cancellationToken);
            if (existing == null)
            {
                _logger.LogInformation("Generated unique code {Code} on attempt {Attempt}", code, attempt + 1);
                return code;
            }

            _logger.LogWarning("Code collision detected: {Code}. Retrying...", code);
        }

        throw new InvalidOperationException("Failed to generate unique code after maximum retries");
    }

    private string GenerateRandomCode()
    {
        var random = RandomNumberGenerator.Create();
        var bytes = new byte[CodeLength];
        random.GetBytes(bytes);

        var chars = new char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            chars[i] = AllowedChars[bytes[i] % AllowedChars.Length];
        }

        return new string(chars);
    }
}