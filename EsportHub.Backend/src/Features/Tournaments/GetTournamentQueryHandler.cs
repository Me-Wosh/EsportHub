using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record GetTournamentQuery(Guid Id) : IQuery<TournamentResult>;

public class GetTournamentQueryHandler(
    EsportHubDbContext dbContext
) : IQueryHandler<GetTournamentQuery, TournamentResult>
{
    public async Task<Result<TournamentResult>> Handle(
        GetTournamentQuery query,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.StartDate,
                t.Status
            })
            .SingleOrDefaultAsync(t => t.Id == query.Id, cancellationToken);

        if (tournament is null)
            return Result.NotFound("Tournament not found.");

        var final = await dbContext.KnockoutStageMatches
            .AsNoTracking()
            .Include(m => m.KnockoutStage)
            .Include(m => m.Team1)
            .Include(m => m.Team2)
            .Where(m => m.Round == KnockoutStageRound.Final)
            .Where(m => m.KnockoutStage.TournamentId == tournament.Id)
            .SingleOrDefaultAsync(cancellationToken);

        var winnerName = final != null && final.IsResolved
            ? (final.WinnerTeamId == final.Team1Id ? final.Team1.Name : final.Team2.Name)
            : null;

        return new TournamentResult(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.Status,
            winnerName
        );
    }
}
