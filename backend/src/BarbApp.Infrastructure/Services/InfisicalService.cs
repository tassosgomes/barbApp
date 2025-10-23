using Infisical.Sdk;
using Infisical.Sdk.Model;
using Microsoft.Extensions.Configuration;

namespace BarbApp.Infrastructure.Services;

public interface ISecretManager
{
    Task<string> GetSecretAsync(string secretName);
}

public class InfisicalService : ISecretManager
{
    private readonly InfisicalClient _infisicalClient;
    private readonly string _environment;
    private readonly string _projectId;

    public InfisicalService(IConfiguration configuration)
    {
        var settings = new InfisicalSdkSettingsBuilder()
            .WithHostUri(configuration["Infisical:HostUri"])
            .Build();

        _infisicalClient = new InfisicalClient(settings);
        _environment = configuration["Infisical:Environment"];
        _projectId = configuration["Infisical:ProjectId"];

        var clientId = configuration["Infisical:ClientId"];
        var clientSecret = configuration["Infisical:ClientSecret"];

        _infisicalClient.Auth().UniversalAuth().LoginAsync(clientId, clientSecret).GetAwaiter().GetResult();
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        var options = new GetSecretOptions
        {
            SecretName = secretName,
            EnvironmentSlug = _environment,
            ProjectId = _projectId,
            SecretPath = "/"
        };

        var secret = await _infisicalClient.Secrets().GetAsync(options);
        return secret.SecretValue;
    }
}