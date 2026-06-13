using EsportHub.Domain.Tournaments;
using EsportHub.Features.Teams.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Features.Teams;

public record UpdateTeamNameCommand(Guid Id, string Name) : ICommand<TeamResult>;

public class UpdateTeamNameCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<UpdateTeamNameCommand, TeamResult>
{
    public async Task<Result<TeamResult>> Handle(UpdateTeamNameCommand command, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams
            .Include(t => t.Tournament)
            .Include(t => t.Players)
            .SingleOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (team is null)
            return Result.NotFound("Team not found.");

        if (team.Tournament.Status != TournamentStatus.InPreparation)
            return Result.Invalid(new ValidationError("Teams can only be updated before the tournament starts."));

        var teamExists = await dbContext.Teams
            .AnyAsync(t =>
                t.TournamentId == team.TournamentId &&
                t.Name == command.Name &&
                t.Id != team.Id,
                cancellationToken);

        if (teamExists)
            return Result.Invalid(new ValidationError("A team with the same name already exists in this tournament."));

        var updateTeamResult = team.UpdateName(command.Name);
        if (!updateTeamResult.IsSuccess)
            return updateTeamResult.Map();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TeamResult(
            team.Id,
            team.Name,
            team.TournamentId,
            team.Tournament.Name,
            team.Players.Select(p => new PlayerResult(p.Id, p.Name)));
    }
}
