using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record GetTournamentsQuery : IQuery<IEnumerable<TournamentResult>>;

public class GetTournamentsQueryHandler(
    EsportHubDbContext dbContext
) : IQueryHandler<GetTournamentsQuery, IEnumerable<TournamentResult>>
{
    public async Task<Result<IEnumerable<TournamentResult>>> Handle(
        GetTournamentsQuery query,
        CancellationToken cancellationToken)
    {
        var tournaments = await dbContext.Tournaments
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.StartDate,
                t.Status
            })
            .ToListAsync(cancellationToken);

        var tournamentIds = tournaments.Select(t => t.Id).ToList();

        var finals = await dbContext.KnockoutStageMatches
            .AsNoTracking()
            .Include(ksm => ksm.KnockoutStage)
            .Include(ksm => ksm.Team1)
            .Include(ksm => ksm.Team2)
            .Where(ksm => ksm.Round == KnockoutStageRound.Final)
            .Where(ksm => tournamentIds.Contains(ksm.KnockoutStage.TournamentId))
            .ToListAsync(cancellationToken);

        var winnerNames = finals
            .Where(f => f.IsResolved)
            .ToDictionary(
                f => f.KnockoutStage.TournamentId,
                f => f.WinnerTeamId == f.Team1Id ? f.Team1.Name : f.Team2.Name
            );

        return tournaments
            .Select(t => new TournamentResult(
                t.Id,
                t.Name,
                t.StartDate,
                t.Status,
                winnerNames.GetValueOrDefault(t.Id)))
            .ToList();
    }
}
