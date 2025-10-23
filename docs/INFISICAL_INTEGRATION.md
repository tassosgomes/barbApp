# Integrating Infisical for Secret Management in BarbApp API

This document provides a step-by-step guide on how to integrate Infisical for managing secrets in the BarbApp API. This will replace the current practice of storing secrets in `appsettings.json` and other configuration files.

## 1. Prerequisites

- An Infisical account and a project created.
- The Infisical CLI installed on your local machine.
- .NET 8 SDK installed.

## 2. Integration Steps

### Step 1: Install the Infisical .NET SDK

Open a terminal in the `backend/src/BarbApp.API` directory and run the following command to install the Infisical .NET SDK:

```bash
dotnet add package Infisical.Sdk
```

### Step 2: Configure the Infisical Client

We will create a new service to encapsulate the Infisical client and provide a simple interface for retrieving secrets. Create a new file named `InfisicalService.cs` in the `backend/src/BarbApp.Infrastructure/Services` directory with the following content:

```csharp
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
    private readonly IInfisicalClient _infisicalClient;
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
```

### Step 3: Register the Infisical Service

In `backend/src/BarbApp.API/Program.cs`, register the `InfisicalService` in the dependency injection container. Add the following lines before `var app = builder.Build();`:

```csharp
builder.Services.AddSingleton<ISecretManager, InfisicalService>();
```

### Step 4: Update `appsettings.json`

Remove all hardcoded secrets from `backend/src/BarbApp.API/appsettings.json` and add the Infisical configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Infisical": {
    "HostUri": "https://app.infisical.com",
    "Environment": "dev",
    "ProjectId": "<your-project-id>",
    "ClientId": "${INFISICAL_CLIENT_ID}",
    "ClientSecret": "${INFISICAL_CLIENT_SECRET}"
  }
}
```

**Note:** The `ClientId` and `ClientSecret` will be injected as environment variables.

### Step 5: Update Code to Use the Secret Manager

Now, refactor the code to use the `ISecretManager` to retrieve secrets instead of `IConfiguration`. For example, in `backend/src/BarbApp.API/Program.cs`, you would change:

```csharp
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
```

to:

```csharp
var secretManager = app.Services.GetRequiredService<ISecretManager>();
var jwtSecret = await secretManager.GetSecretAsync("JwtSettings:Secret");
```

This change will need to be applied to all places where secrets are currently being read from `IConfiguration`.

### Step 6: Running the Application with Infisical

To run the application locally, you can use the Infisical CLI to inject the secrets as environment variables. Run the following command from the `backend/src/BarbApp.API` directory:

```bash
infisical run -- dotnet run
```

This command will fetch the secrets from your Infisical project and inject them as environment variables before running the application.

## 3. Conclusion

By following these steps, you can successfully integrate Infisical into the BarbApp API for secure secret management. This will eliminate the risk of exposing secrets in the codebase and provide a centralized and secure way to manage them.
