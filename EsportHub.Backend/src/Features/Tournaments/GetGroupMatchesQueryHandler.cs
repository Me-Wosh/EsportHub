using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record GetGroupMatchesQuery(Guid TournamentId, Guid GroupId) : IQuery<IEnumerable<GroupMatchResult>>;

public class GetGroupMatchesQueryHandler(
    EsportHubDbContext dbContext
) : IQueryHandler<GetGroupMatchesQuery, IEnumerable<GroupMatchResult>>
{
    public async Task<Result<IEnumerable<GroupMatchResult>>> Handle(
        GetGroupMatchesQuery query,
        CancellationToken cancellationToken)
    {
        var groupExists = await dbContext.Groups
            .AnyAsync(g => g.Id == query.GroupId && g.GroupStage.TournamentId == query.TournamentId, cancellationToken);

        if (!groupExists)
            return Result.NotFound("Group not found.");

        var matches = await dbContext.GroupStageMatches
            .AsNoTracking()
            .Where(m => m.GroupId == query.GroupId)
            .Select(m => new GroupMatchResult(
                m.Id,
                m.Team1Id,
                m.Team1.Name,
                m.Team1Score,
                m.Team2Id,
                m.Team2.Name,
                m.Team2Score,
                m.Team1Score != null && m.Team2Score != null
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<GroupMatchResult>>(matches);
    }
}
