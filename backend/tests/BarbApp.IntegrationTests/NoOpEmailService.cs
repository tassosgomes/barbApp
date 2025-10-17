using BarbApp.Application.Interfaces;

namespace BarbApp.IntegrationTests;

/// <summary>
/// No-op implementation of IEmailService for integration tests.
/// This prevents actual emails from being sent during tests.
/// </summary>
public class NoOpEmailService : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        // Do nothing - just return completed task
        return Task.CompletedTask;
    }
}
