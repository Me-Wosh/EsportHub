using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record StartGroupStageCommand(Guid TournamentId, List<string> GroupNames) : ICommand<GroupStageResult>;

public class StartGroupStageCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<StartGroupStageCommand, GroupStageResult>
{
    public async Task<Result<GroupStageResult>> Handle(
        StartGroupStageCommand command,
        CancellationToken cancellationToken)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.GroupStage)
            .SingleOrDefaultAsync(t => t.Id == command.TournamentId, cancellationToken);

        if (tournament is null)
            return Result.NotFound("Tournament not found.");

        var teams = await dbContext.Teams
            .Include(t => t.Players)
            .Where(t => t.TournamentId == command.TournamentId)
            .ToListAsync(cancellationToken);

        var startTournamentResult = tournament.Start(teams, command.GroupNames);
        if (!startTournamentResult.IsSuccess)
            return startTournamentResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Created(new GroupStageResult(
            tournament.GroupStage!.Id,
            tournament.GroupStage.IsClosed,
            tournament.GroupStage.Groups.Select(g => new GroupSummaryResult(
                g.Id,
                g.Name,
                g.Teams.Select(t => new TeamSummaryResult(t.Id, t.Name))
            ))
        ));
    }
}
