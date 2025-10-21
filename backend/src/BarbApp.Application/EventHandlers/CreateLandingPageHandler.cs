using MediatR;
using Polly;
using BarbApp.Application.Interfaces.UseCases;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BarbApp.Application.EventHandlers
{
    public class CreateLandingPageHandler : INotificationHandler<Domain.Events.BarbershopCreatedEvent>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CreateLandingPageHandler> _logger;

        public CreateLandingPageHandler(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CreateLandingPageHandler> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task Handle(Domain.Events.BarbershopCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Iniciando criação automática de landing page para barbearia {BarbershopId}",
                notification.BarbershopId);

            // Retry policy: 3 tentativas com intervalo exponencial
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Tentativa {RetryCount} de criar landing page falhou. Aguardando {TimeSpan} antes de tentar novamente",
                            retryCount,
                            timeSpan);
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var landingPageService = scope.ServiceProvider.GetRequiredService<ILandingPageService>();

                try
                {
                    await landingPageService.CreateAsync(notification.BarbershopId, cancellationToken);

                    _logger.LogInformation(
                        "Landing page criada automaticamente com sucesso para barbearia {BarbershopId}",
                        notification.BarbershopId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Falha ao criar landing page automaticamente para barbearia {BarbershopId}",
                        notification.BarbershopId);

                    throw; // Re-throw to let Polly handle retry
                }
            });
        }
    }
}