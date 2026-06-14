using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record GetGroupsQuery(Guid TournamentId) : IQuery<IEnumerable<GroupResult>>;

public class GetGroupsQueryHandler(
    EsportHubDbContext dbContext
) : IQueryHandler<GetGroupsQuery, IEnumerable<GroupResult>>
{
    public async Task<Result<IEnumerable<GroupResult>>> Handle(
        GetGroupsQuery query,
        CancellationToken cancellationToken)
    {
        var tournamentExists = await dbContext.Tournaments
            .AnyAsync(t => t.Id == query.TournamentId, cancellationToken);

        if (!tournamentExists)
            return Result.NotFound("Tournament not found.");

        var groups = await dbContext.Groups
            .AsNoTracking()
            .Where(g => g.GroupStage.TournamentId == query.TournamentId)
            .Select(g => new GroupResult(
                g.Id,
                g.Name,
                g.Standings.OrderBy(s => s.Position).Select(s => new GroupTeamStandingResult(
                    s.Position,
                    s.TeamId,
                    s.Team.Name,
                    s.GamesPlayed,
                    s.Wins,
                    s.Losses,
                    s.PointsFor,
                    s.PointsAgainst
                ))
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<GroupResult>>(groups);
    }
}
