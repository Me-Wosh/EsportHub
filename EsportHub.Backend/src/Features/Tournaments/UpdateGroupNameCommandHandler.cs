using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Tournaments;

public record UpdateGroupNameCommand(Guid TournamentId, Guid GroupId, string Name) : ICommand<GroupResult>;

public class UpdateGroupNameCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<UpdateGroupNameCommand, GroupResult>
{
    public async Task<Result<GroupResult>> Handle(
        UpdateGroupNameCommand command,
        CancellationToken cancellationToken)
    {
        var group = await dbContext.Groups
            .Include(g => g.GroupStage)
            .Include(g => g.Standings)
                .ThenInclude(s => s.Team)
            .SingleOrDefaultAsync(
                g => g.Id == command.GroupId && g.GroupStage.TournamentId == command.TournamentId,
                cancellationToken);

        if (group is null)
            return Result.NotFound("Group not found.");

        if (group.GroupStage.IsClosed)
            return Result.Invalid(new ValidationError("Group name cannot be updated after the group stage is closed."));

        var updateGroupNameResult = group.UpdateName(command.Name);
        if (!updateGroupNameResult.IsSuccess)
            return updateGroupNameResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new GroupResult(
            group.Id,
            group.Name,
            group.Standings
            .OrderBy(s => s.Position).Select(s => new GroupTeamStandingResult(
                s.Position,
                s.TeamId,
                s.Team.Name,
                s.GamesPlayed,
                s.Wins,
                s.Losses,
                s.PointsFor,
                s.PointsAgainst))
        );
    }
}
