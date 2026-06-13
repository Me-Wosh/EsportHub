using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments.DTOs;
using EsportHub.Infrastructure.MediatR;
using EsportHub.Persistence;

namespace EsportHub.Features.Tournaments;

public record CreateTournamentCommand(string Name) : ICommand<TournamentResult>;

public class CreateTournamentCommandHandler(
    EsportHubDbContext dbContext
) : ICommandHandler<CreateTournamentCommand, TournamentResult>
{
    public async Task<Result<TournamentResult>> Handle(
        CreateTournamentCommand command,
        CancellationToken cancellationToken)
    {
        var createTournamentResult = Tournament.Create(command.Name);
        if (!createTournamentResult.IsSuccess)
            return createTournamentResult.Map();

        var tournament = createTournamentResult.Value;
        dbContext.Tournaments.Add(tournament);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Created(new TournamentResult(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.Status,
            null
        ));
    }
}
