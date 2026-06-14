using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record ResolveGroupStageMatchCommand(
    Guid TournamentId,
    Guid GroupId,
    Guid MatchId,
    int Team1Score,
    int Team2Score
) : ICommand<GroupMatchResult>;

public class ResolveGroupStageMatchCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<ResolveGroupStageMatchCommand, GroupMatchResult>
{
    public async Task<Result<GroupMatchResult>> Handle(
        ResolveGroupStageMatchCommand command,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Matches)
                        .ThenInclude(m => m.Team1)
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Matches)
                        .ThenInclude(m => m.Team2)
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

        var resolveMatchResult = tournament.GroupStage.ResolveMatch(
            command.GroupId,
            command.MatchId,
            command.Team1Score,
            command.Team2Score);

        if (!resolveMatchResult.IsSuccess)
            return resolveMatchResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        var match = resolveMatchResult.Value;
        
        return new GroupMatchResult(
            match.Id,
            match.Team1Id,
            match.Team1.Name,
            match.Team1Score,
            match.Team2Id,
            match.Team2.Name,
            match.Team2Score,
            match.IsResolved);
    }
}
