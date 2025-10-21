using BarbApp.Domain.Interfaces.Repositories;

namespace BarbApp.Application.Interfaces;

public interface IUnitOfWork
{
    ILandingPageConfigRepository LandingPageConfigs { get; }
    ILandingPageServiceRepository LandingPageServices { get; }
    
    Task Commit(CancellationToken cancellationToken);
    Task Rollback(CancellationToken cancellationToken);
}