using EsportHub.Domain.Tournaments;
using EsportHub.Features.Teams.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record UpdatePlayerNameCommand(Guid TeamId, Guid PlayerId, string Name) : ICommand<PlayerResult>;

public class UpdatePlayerNameCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<UpdatePlayerNameCommand, PlayerResult>
{
    public async Task<Result<PlayerResult>> Handle(UpdatePlayerNameCommand command, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams
            .Include(t => t.Tournament)
            .Include(t => t.Players)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken);

        if (team is null)
            return Result.NotFound("Team not found.");

        if (team.Tournament.Status != TournamentStatus.InPreparation)
            return Result.Invalid(new ValidationError("Players can only be updated before the tournament starts."));

        var updatePlayerNameResult = team.UpdatePlayerName(command.PlayerId, command.Name);
        if (!updatePlayerNameResult.IsSuccess)
            return updatePlayerNameResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        var player = updatePlayerNameResult.Value;
        return new PlayerResult(player.Id, player.Name);
    }
}
