using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using EsportHub.Features.Teams;
using EsportHub.UnitTests.Features;

namespace EsportHub.UnitTests.Features.Teams;

public class SeedTeamsCommandHandlerTests : HandlerTestBase
{
    private readonly SeedTeamsCommandHandler _handler;

    public SeedTeamsCommandHandlerTests()
    {
        _handler = new SeedTeamsCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new SeedTeamsCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentNotInPreparation_ReturnsInvalid()
    {
        var (tournament, _) = await SeedGroupStageAsync();

        var result = await _handler.Handle(
            new SeedTeamsCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenMaxTeamsAlreadyReached_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        for (var i = 0; i < TournamentConstraints.TeamsRequiredCount; i++)
            await SeedTeamAsync(tournament.Id, $"Team {i + 1}");

        var result = await _handler.Handle(
            new SeedTeamsCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenEmptyTournament_ReturnsSeededTeams()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new SeedTeamsCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TournamentConstraints.TeamsRequiredCount, result.Value.Count());
    }

    [Fact]
    public async Task Handle_GivenEmptyTournament_SeedsTeamsWithMinimumPlayers()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new SeedTeamsCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value, t => Assert.Equal(TeamConstraints.PlayersMinCount, t.Players.Count()));
    }

    [Fact]
    public async Task Handle_GivenTournamentWithSomeTeams_FillsRemainingSlots()
    {
        var tournament = await SeedTournamentAsync();
        var existingCount = 5;
        for (var i = 0; i < existingCount; i++)
            await SeedTeamAsync(tournament.Id, $"Existing Team {i + 1}");

        var result = await _handler.Handle(
            new SeedTeamsCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TournamentConstraints.TeamsRequiredCount - existingCount, result.Value.Count());
        Assert.Equal(TournamentConstraints.TeamsRequiredCount, Context.Teams.Count());
    }
}
