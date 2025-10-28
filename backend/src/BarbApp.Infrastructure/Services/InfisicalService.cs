using Infisical.Sdk;
using Infisical.Sdk.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BarbApp.Infrastructure.Services;

public interface ISecretManager
{
    /// <summary>
    /// Retrieves a secret value from Infisical by its name.
    /// The secret name should be the flat name as stored in Infisical (e.g., "JWT_SECRET", not "JwtSettings:Secret").
    /// </summary>
    /// <param name="secretName">The name of the secret in Infisical</param>
    /// <returns>The secret value as a string</returns>
    Task<string> GetSecretAsync(string secretName);
}

public class InfisicalService : ISecretManager
{
    private readonly InfisicalClient? _infisicalClient;
    private readonly string? _environment;
    private readonly string? _projectId;
    private readonly ILogger<InfisicalService> _logger;

    public InfisicalService(IConfiguration configuration, ILogger<InfisicalService> logger)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        
        // Skip Infisical authentication in testing environment
        if (environment == "Testing")
        {
            _logger = logger;
            _logger.LogInformation("Skipping Infisical authentication in testing environment");
            return;
        }

        var settings = new InfisicalSdkSettingsBuilder()
            .WithHostUri(configuration["Infisical:HostUri"])
            .Build();

        _infisicalClient = new InfisicalClient(settings);
        _environment = configuration["Infisical:Environment"];
        _projectId = configuration["Infisical:ProjectId"];
        _logger = logger;

        var clientId = configuration["Infisical:ClientId"];
        var clientSecret = configuration["Infisical:ClientSecret"];

        try
        {
            _infisicalClient.Auth().UniversalAuth().LoginAsync(clientId, clientSecret).GetAwaiter().GetResult();
            _logger.LogInformation("Infisical authentication successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Infisical authentication failed");
            throw;
        }
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        // Return test values in testing environment
        if (_infisicalClient == null)
        {
            return secretName switch
            {
                "JWT_SECRET" => "test-secret-key-at-least-32-characters-long-for-jwt",
                _ => $"test-value-for-{secretName}"
            };
        }

        try
        {
            var options = new GetSecretOptions
            {
                SecretName = secretName,
                EnvironmentSlug = _environment,
                ProjectId = _projectId
                // SecretPath defaults to root when not specified
            };

            var secret = await _infisicalClient.Secrets().GetAsync(options);
            _logger.LogInformation($"Secret '{secretName}' retrieved successfully");
            return secret.SecretValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve secret '{secretName}'");
            throw;
        }
    }
}

public class TestSecretManager : ISecretManager
{
    public Task<string> GetSecretAsync(string secretName)
    {
        return Task.FromResult(secretName switch
        {
            "JWT_SECRET" => "test-secret-key-at-least-32-characters-long-for-jwt",
            _ => $"test-value-for-{secretName}"
        });
    }
}