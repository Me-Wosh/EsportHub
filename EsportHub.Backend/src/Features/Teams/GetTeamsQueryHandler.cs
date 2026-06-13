using EsportHub.Features.Teams.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record GetTeamsQuery(Guid? TournamentId) : IQuery<IEnumerable<TeamResult>>;

public class GetTeamsQueryHandler(EsportHubDbContext dbContext) : IQueryHandler<GetTeamsQuery, IEnumerable<TeamResult>>
{
    public async Task<Result<IEnumerable<TeamResult>>> Handle(GetTeamsQuery query, CancellationToken cancellationToken)
    {
        if (query.TournamentId.HasValue)
        {
            var tournamentExists = await dbContext.Tournaments
                .AnyAsync(t => t.Id == query.TournamentId.Value, cancellationToken);

            if (!tournamentExists)
                return Result.NotFound("Tournament not found.");
        }

        var teams = await dbContext.Teams
            .AsNoTracking()
            .Include(t => t.Tournament)
            .Include(t => t.Players)
            .Where(t => query.TournamentId == null || t.TournamentId == query.TournamentId)
            .ToListAsync(cancellationToken);

        var teamResults = teams
            .Select(t => new TeamResult(
                t.Id,
                t.Name,
                t.TournamentId,
                t.Tournament.Name,
                t.Players.Select(p => new PlayerResult(p.Id, p.Name))))
            .ToList();

        return teamResults;
    }
}
