using EsportHub.Features.Teams;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.UnitTests.Features.Teams;

public class RemovePlayerCommandHandlerTests : HandlerTestBase
{
    private readonly RemovePlayerCommandHandler _handler;

    public RemovePlayerCommandHandlerTests()
    {
        _handler = new RemovePlayerCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTeamId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new RemovePlayerCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentNotInPreparation_ReturnsInvalid()
    {
        var (_, groupStage) = await SeedGroupStageAsync();
        var team = groupStage.Groups.First().Teams.First();

        var result = await _handler.Handle(
            new RemovePlayerCommand(team.Id, Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNonExistentPlayerId_ReturnsNotFound()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        var result = await _handler.Handle(
            new RemovePlayerCommand(team.Id, Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenValidTeamAndPlayer_ReturnsSuccess()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addHandler = new AddPlayerCommandHandler(Context);
        var addResult = await addHandler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);

        var result = await _handler.Handle(
            new RemovePlayerCommand(team.Id, addResult.Value.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_GivenValidTeamAndPlayer_PersistsRemoval()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addHandler = new AddPlayerCommandHandler(Context);
        var addResult = await addHandler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);

        await _handler.Handle(
            new RemovePlayerCommand(team.Id, addResult.Value.Id),
            CancellationToken.None);

        Assert.Equal(0, Context.Players.Count());
    }
}
