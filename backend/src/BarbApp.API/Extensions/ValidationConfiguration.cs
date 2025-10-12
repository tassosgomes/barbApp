using FluentValidation;
using FluentValidation.AspNetCore;
using BarbApp.Application.Validators;

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

        // Register validators
        services.AddValidatorsFromAssemblyContaining<LoginAdminCentralInputValidator>();

        return services;
    }
}