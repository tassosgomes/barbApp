using BarbApp.Application.DTOs;

namespace BarbApp.Application.Interfaces.UseCases;

public interface IGetTeamScheduleUseCase
{
    Task<TeamScheduleOutput> ExecuteAsync(DateTime date, Guid? barberId, CancellationToken cancellationToken);
}