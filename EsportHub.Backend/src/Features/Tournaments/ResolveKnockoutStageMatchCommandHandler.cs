using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record ResolveKnockoutStageMatchCommand(
    Guid TournamentId,
    Guid MatchId,
    int Team1Score,
    int Team2Score
) : ICommand<KnockoutMatchResult>;

public class ResolveKnockoutStageMatchCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<ResolveKnockoutStageMatchCommand, KnockoutMatchResult>
{
    public async Task<Result<KnockoutMatchResult>> Handle(
        ResolveKnockoutStageMatchCommand command,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.KnockoutStage!)
                .ThenInclude(ks => ks.Matches)
                    .ThenInclude(m => m.Team1)
            .Include(t => t.KnockoutStage!)
                .ThenInclude(ks => ks.Matches)
                    .ThenInclude(m => m.Team2)
            .SingleOrDefaultAsync(t => t.Id == command.TournamentId, cancellationToken);

        if (tournament is null)
            return Result.NotFound("Tournament not found.");

        if (tournament.KnockoutStage is null)
            return Result.NotFound("Knockout stage not found.");

        var match = tournament.KnockoutStage.Matches.SingleOrDefault(m => m.Id == command.MatchId);
        if (match is null)
            return Result.NotFound("Match not found in knockout stage.");

        var resolveKnockoutStageMatchResult = tournament.KnockoutStage.ResolveMatch(
            match,
            command.Team1Score,
            command.Team2Score);

        if (!resolveKnockoutStageMatchResult.IsSuccess)
            return resolveKnockoutStageMatchResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new KnockoutMatchResult(
            match.Id,
            match.Round,
            match.Side,
            match.Team1Id,
            match.Team1.Name,
            match.Team1Score,
            match.Team2Id,
            match.Team2.Name,
            match.Team2Score,
            match.IsResolved);
    }
}
