using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record GetGroupQuery(Guid TournamentId, Guid GroupId) : IQuery<GroupResult>;

public class GetGroupQueryHandler(EsportHubDbContext dbContext) : IQueryHandler<GetGroupQuery, GroupResult>
{
    public async Task<Result<GroupResult>> Handle(GetGroupQuery query, CancellationToken cancellationToken)
    {
        var group = await dbContext.Groups
            .AsNoTracking()
            .Where(g => g.Id == query.GroupId && g.GroupStage.TournamentId == query.TournamentId)
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
            .SingleOrDefaultAsync(cancellationToken);

        if (group is null)
            return Result.NotFound("Group not found.");

        return group;
    }
}
