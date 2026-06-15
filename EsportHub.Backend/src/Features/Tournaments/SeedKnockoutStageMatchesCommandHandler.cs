using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record SeedKnockoutStageMatchesCommand(Guid TournamentId) : ICommand<KnockoutStageResult>;

public class SeedKnockoutStageMatchesCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<SeedKnockoutStageMatchesCommand, KnockoutStageResult>
{
    public async Task<Result<KnockoutStageResult>> Handle(
        SeedKnockoutStageMatchesCommand command,
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

        if (tournament.KnockoutStage.IsClosed)
            return Result.Invalid(new ValidationError("Cannot seed matches after the knockout stage is closed."));

        while (!tournament.KnockoutStage.IsClosed)
        {
            var unresolvedMatches = tournament.KnockoutStage.Matches
                .Where(m => !m.IsResolved)
                .ToList();

            if (unresolvedMatches.Count == 0)
                break;

            foreach (var match in unresolvedMatches)
            {
                var (team1Score, team2Score) = match.Team1Id.CompareTo(match.Team2Id) < 0 ? (3, 1) : (1, 3);

                var resolveResult = tournament.KnockoutStage.ResolveMatch(match, team1Score, team2Score);
                if (!resolveResult.IsSuccess)
                    return resolveResult.Map();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new KnockoutStageResult(
            tournament.KnockoutStage.Id,
            tournament.KnockoutStage.IsClosed,
            tournament.KnockoutStage.Matches.Select(m => new KnockoutMatchResult(
                m.Id,
                m.Round,
                m.Side,
                m.Team1Id,
                m.Team1.Name,
                m.Team1Score,
                m.Team2Id,
                m.Team2.Name,
                m.Team2Score,
                m.IsResolved))
        );
    }
}
