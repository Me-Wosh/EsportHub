using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using EsportHub.Features.Teams.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record CreateTeamCommand(Guid TournamentId, string Name) : ICommand<TeamResult>;

public class CreateTeamCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<CreateTeamCommand, TeamResult>
{
    public async Task<Result<TeamResult>> Handle(
        CreateTeamCommand command,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == command.TournamentId, cancellationToken);

        if (tournament is null)
            return Result.NotFound("Tournament not found.");

        if (tournament.Status != TournamentStatus.InPreparation)
            return Result.Invalid(new ValidationError("Teams can only be created before the tournament starts."));

        var teamCount = await dbContext.Teams
            .CountAsync(t => t.TournamentId == command.TournamentId, cancellationToken);

        if (teamCount >= TournamentConstraints.TeamsRequiredCount)
        {
            return Result.Invalid(
                new ValidationError("The maximum number of teams for this tournament has been reached."));
        }

        var teamExists = await dbContext.Teams
            .AnyAsync(t => t.TournamentId == command.TournamentId && t.Name == command.Name, cancellationToken);

        if (teamExists)
            return Result.Invalid(new ValidationError("A team with the same name already exists in this tournament."));

        var createTeamResult = Team.Create(command.Name, command.TournamentId);
        if (!createTeamResult.IsSuccess)
            return createTeamResult.Map();

        var team = createTeamResult.Value;
        dbContext.Teams.Add(team);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Created(new TeamResult(
            team.Id,
            team.Name,
            team.TournamentId,
            tournament.Name,
            []));
    }
}
