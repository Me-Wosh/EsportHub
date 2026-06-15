using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record GetKnockoutStageQuery(Guid TournamentId) : IQuery<KnockoutStageResult>;

public class GetKnockoutStageQueryHandler(
    EsportHubDbContext dbContext
) : IQueryHandler<GetKnockoutStageQuery, KnockoutStageResult>
{
    public async Task<Result<KnockoutStageResult>> Handle(
        GetKnockoutStageQuery query,
        CancellationToken cancellationToken)
    {
        var knockoutStage = await dbContext.KnockoutStages
            .AsNoTracking()
            .Include(ks => ks.Matches)
                .ThenInclude(m => m.Team1)
            .Include(ks => ks.Matches)
                .ThenInclude(m => m.Team2)
            .SingleOrDefaultAsync(ks => ks.TournamentId == query.TournamentId, cancellationToken);

        if (knockoutStage is null)
            return Result.NotFound("Knockout stage not found.");

        return new KnockoutStageResult(
            knockoutStage.Id,
            knockoutStage.IsClosed,
            knockoutStage.Matches.Select(m => new KnockoutMatchResult(
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
