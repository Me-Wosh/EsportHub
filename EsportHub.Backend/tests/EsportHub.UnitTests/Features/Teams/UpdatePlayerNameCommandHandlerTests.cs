using EsportHub.Domain.Teams;
using EsportHub.Features.Teams;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.UnitTests.Features.Teams;

public class UpdatePlayerNameCommandHandlerTests : HandlerTestBase
{
    private readonly UpdatePlayerNameCommandHandler _handler;
    private readonly AddPlayerCommandHandler _addPlayerHandler;

    public UpdatePlayerNameCommandHandlerTests()
    {
        _handler = new UpdatePlayerNameCommandHandler(Context);
        _addPlayerHandler = new AddPlayerCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTeamId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new UpdatePlayerNameCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name"),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentNotInPreparation_ReturnsInvalid()
    {
        var (_, groupStage) = await SeedGroupStageAsync();
        var team = groupStage.Groups.First().Teams.First();

        var result = await _handler.Handle(
            new UpdatePlayerNameCommand(team.Id, Guid.NewGuid(), "New Name"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNonExistentPlayerId_ReturnsNotFound()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        var result = await _handler.Handle(
            new UpdatePlayerNameCommand(team.Id, Guid.NewGuid(), "New Name"),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenValidName_ReturnsUpdatedPlayer()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addResult = await _addPlayerHandler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);

        var result = await _handler.Handle(
            new UpdatePlayerNameCommand(team.Id, addResult.Value.Id, "Player Alpha"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Player Alpha", result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenValidName_PersistsUpdatedName()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addResult = await _addPlayerHandler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);

        await _handler.Handle(
            new UpdatePlayerNameCommand(team.Id, addResult.Value.Id, "Player Alpha"),
            CancellationToken.None);

        var player = await Context.Players.SingleAsync(p => p.Id == addResult.Value.Id);
        Assert.Equal("Player Alpha", player.Name);
    }

    [Fact]
    public async Task Handle_GivenEmptyName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addResult = await _addPlayerHandler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);

        var result = await _handler.Handle(
            new UpdatePlayerNameCommand(team.Id, addResult.Value.Id, ""),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addResult = await _addPlayerHandler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);
        var longName = new string('A', PlayerConstraints.NameMaxLength + 1);

        var result = await _handler.Handle(
            new UpdatePlayerNameCommand(team.Id, addResult.Value.Id, longName),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenDuplicatePlayerName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        await _addPlayerHandler.Handle(new AddPlayerCommand(team.Id, "Player One"), CancellationToken.None);
        var addResult = await _addPlayerHandler.Handle(
            new AddPlayerCommand(team.Id, "Player Two"),
            CancellationToken.None);

        var result = await _handler.Handle(
            new UpdatePlayerNameCommand(team.Id, addResult.Value.Id, "Player One"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
