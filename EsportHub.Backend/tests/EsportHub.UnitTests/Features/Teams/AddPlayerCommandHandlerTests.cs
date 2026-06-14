using EsportHub.Domain.Teams;
using EsportHub.Features.Teams;
using EsportHub.UnitTests.Features;

namespace EsportHub.UnitTests.Features.Teams;

public class AddPlayerCommandHandlerTests : HandlerTestBase
{
    private readonly AddPlayerCommandHandler _handler;

    public AddPlayerCommandHandlerTests()
    {
        _handler = new AddPlayerCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTeamId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new AddPlayerCommand(Guid.NewGuid(), "Player One"),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenValidNameAndTeam_ReturnsCreatedPlayer()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        var result = await _handler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Player One", result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenValidName_PersistsPlayerToDatabase()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        await _handler.Handle(new AddPlayerCommand(team.Id, "Player One"), CancellationToken.None);

        Assert.Equal(1, Context.Players.Count());
    }

    [Fact]
    public async Task Handle_GivenEmptyPlayerName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        var result = await _handler.Handle(
            new AddPlayerCommand(team.Id, ""),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var longName = new string('A', PlayerConstraints.NameMaxLength + 1);

        var result = await _handler.Handle(
            new AddPlayerCommand(team.Id, longName),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenDuplicatePlayerName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        await _handler.Handle(new AddPlayerCommand(team.Id, "Player One"), CancellationToken.None);

        var result = await _handler.Handle(
            new AddPlayerCommand(team.Id, "Player One"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenMaxPlayersAlreadyInTeam_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        for (var i = 0; i < TeamConstraints.PlayersMaxCount; i++)
            await _handler.Handle(new AddPlayerCommand(team.Id, $"Player {i + 1}"), CancellationToken.None);

        var result = await _handler.Handle(
            new AddPlayerCommand(team.Id, "Extra Player"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
