using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record CloseGroupStageCommand(Guid TournamentId) : ICommand<GroupStageResult>;

public class CloseGroupStageCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<CloseGroupStageCommand, GroupStageResult>
{
    public async Task<Result<GroupStageResult>> Handle(
        CloseGroupStageCommand command,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Matches)
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Standings)
            .Include(t => t.GroupStage!)
                .ThenInclude(gs => gs.Groups)
                    .ThenInclude(g => g.Teams)
            .Include(t => t.KnockoutStage!)
                .ThenInclude(ks => ks.Matches)
            .SingleOrDefaultAsync(t => t.Id == command.TournamentId, cancellationToken);

        if (tournament is null)
            return Result.NotFound("Tournament not found.");

        var closeGroupStageResult = tournament.CloseGroupStage();
        if (!closeGroupStageResult.IsSuccess)
            return closeGroupStageResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new GroupStageResult(
            tournament.GroupStage!.Id,
            tournament.GroupStage.IsClosed,
            tournament.GroupStage.Groups.Select(g => new GroupSummaryResult(
                g.Id,
                g.Name,
                g.Teams.Select(t => new TeamSummaryResult(t.Id, t.Name))
            ))
        );
    }
}
