using EsportHub.Features.Teams.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record GetTeamQuery(Guid Id) : IQuery<TeamResult>;

public class GetTeamQueryHandler(EsportHubDbContext dbContext) : IQueryHandler<GetTeamQuery, TeamResult>
{
    public async Task<Result<TeamResult>> Handle(GetTeamQuery query, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams
            .AsNoTracking()
            .Include(t => t.Tournament)
            .Include(t => t.Players)
            .SingleOrDefaultAsync(t => t.Id == query.Id, cancellationToken);

        if (team is null)
            return Result.NotFound("Team not found.");

        return new TeamResult(
            team.Id,
            team.Name,
            team.TournamentId,
            team.Tournament.Name,
            team.Players.Select(p => new PlayerResult(p.Id, p.Name)));
    }
}
