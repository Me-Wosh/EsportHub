using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record SeedGroupStageMatchesCommand(Guid TournamentId) : ICommand<GroupStageResult>;

public class SeedGroupStageMatchesCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<SeedGroupStageMatchesCommand, GroupStageResult>
{
    public async Task<Result<GroupStageResult>> Handle(
        SeedGroupStageMatchesCommand command,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Matches)
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Standings)
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Teams)
            .SingleOrDefaultAsync(t => t.Id == command.TournamentId, cancellationToken);

        if (tournament is null)
            return Result.NotFound("Tournament not found.");

        if (tournament.GroupStage is null)
            return Result.NotFound("Group stage not found.");

        if (tournament.GroupStage.IsClosed)
            return Result.Invalid(new ValidationError("Cannot seed matches after the group stage is closed."));

        foreach (var group in tournament.GroupStage.Groups)
        {
            var teamRank = group.Teams
                .OrderBy(t => t.Id)
                .Select((t, i) => (t.Id, Rank: i))
                .ToDictionary(x => x.Id, x => x.Rank);

            var unresolvedMatches = group.Matches.Where(m => !m.IsResolved);

            foreach (var match in unresolvedMatches)
            {
                var (team1Score, team2Score) = teamRank[match.Team1Id] < teamRank[match.Team2Id]
                    ? (3, 1)
                    : (1, 3);

                var resolveResult = tournament.GroupStage.ResolveMatch(
                    group.Id,
                    match.Id,
                    team1Score,
                    team2Score);

                if (!resolveResult.IsSuccess)
                    return resolveResult.Map();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new GroupStageResult(
            tournament.GroupStage.Id,
            tournament.GroupStage.IsClosed,
            tournament.GroupStage.Groups.Select(g => new GroupSummaryResult(
                g.Id,
                g.Name,
                g.Teams.Select(t => new TeamSummaryResult(t.Id, t.Name))
            ))
        );
    }
}
