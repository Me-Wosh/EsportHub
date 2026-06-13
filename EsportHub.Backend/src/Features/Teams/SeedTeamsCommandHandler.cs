using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using EsportHub.Features.Teams.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record SeedTeamsCommand(Guid TournamentId) : ICommand<IEnumerable<TeamResult>>;

public class SeedTeamsCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<SeedTeamsCommand, IEnumerable<TeamResult>>
{
    public async Task<Result<IEnumerable<TeamResult>>> Handle(
        SeedTeamsCommand command,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == command.TournamentId, cancellationToken);

        if (tournament is null)
            return Result.NotFound("Tournament not found.");

        if (tournament.Status != TournamentStatus.InPreparation)
            return Result.Invalid(new ValidationError("Teams can only be seeded before the tournament starts."));

        var existingTeamsCount = await dbContext.Teams
            .CountAsync(t => t.TournamentId == command.TournamentId, cancellationToken);

        if (existingTeamsCount == TournamentConstraints.TeamsRequiredCount)
        {
            return Result.Invalid(
                new ValidationError("The maximum number of teams for this tournament has been reached."));
        }

        var teams = new List<Team>();

        for (var i = 1; i <= TournamentConstraints.TeamsRequiredCount - existingTeamsCount; i++)
        {
            var createTeamResult = Team.Create($"Team {i}", command.TournamentId);
            if (!createTeamResult.IsSuccess)
                return createTeamResult.Map();

            var team = createTeamResult.Value;

            for (var j = 1; j <= TeamConstraints.PlayersMinCount; j++)
            {
                var addPlayerResult = team.AddPlayer($"Player {j}");
                if (!addPlayerResult.IsSuccess)
                    return addPlayerResult.Map();
            }

            teams.Add(team);
        }

        await dbContext.Teams.AddRangeAsync(teams, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var teamResults = teams.Select(t => new TeamResult(
            t.Id,
            t.Name,
            t.TournamentId,
            tournament.Name,
            t.Players.Select(p => new PlayerResult(p.Id, p.Name))));

        return Result.Created(teamResults);
    }
}
