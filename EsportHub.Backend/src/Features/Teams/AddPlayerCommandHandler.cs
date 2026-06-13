using EsportHub.Domain.Tournaments;
using EsportHub.Features.Teams.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record AddPlayerCommand(Guid TeamId, string Name) : ICommand<PlayerResult>;

public class AddPlayerCommandHandler(EsportHubDbContext dbContext) : ICommandHandler<AddPlayerCommand, PlayerResult>
{
    public async Task<Result<PlayerResult>> Handle(AddPlayerCommand command, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams
            .Include(t => t.Tournament)
            .Include(t => t.Players)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken);

        if (team is null)
            return Result.NotFound("Team not found.");

        if (team.Tournament.Status != TournamentStatus.InPreparation)
            return Result.Invalid(new ValidationError("Players can only be added before the tournament starts."));

        var addPlayerResult = team.AddPlayer(command.Name);
        if (!addPlayerResult.IsSuccess)
            return addPlayerResult.Map();

        var player = addPlayerResult.Value;
        dbContext.Players.Add(player);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Created(new PlayerResult(player.Id, player.Name));
    }
}
