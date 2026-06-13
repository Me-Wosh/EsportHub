using EsportHub.Domain.Tournaments;
using EsportHub.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record RemovePlayerCommand(Guid TeamId, Guid PlayerId) : IRequest<Result<Unit>>;

public class RemovePlayerCommandHandler(
    EsportHubDbContext dbContext
) : IRequestHandler<RemovePlayerCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RemovePlayerCommand command, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams
            .Include(t => t.Tournament)
            .Include(t => t.Players)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken);

        if (team is null)
            return Result.NotFound("Team not found.");

        if (team.Tournament.Status != TournamentStatus.InPreparation)
            return Result.Invalid(new ValidationError("Players can only be removed before the tournament starts."));

        var player = team.Players.SingleOrDefault(p => p.Id == command.PlayerId);

        if (player is null)
            return Result.NotFound("Player not found in this team.");

        team.RemovePlayer(command.PlayerId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
