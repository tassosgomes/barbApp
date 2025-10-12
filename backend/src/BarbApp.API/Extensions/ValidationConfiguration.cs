using FluentValidation.AspNetCore;

namespace BarbApp.API.Extensions;

public static class ValidationConfiguration
{
    public static IServiceCollection AddFluentValidationConfiguration(
        this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(config =>
        {
            config.DisableDataAnnotationsValidation = true;
        });

        return services;
    }
}