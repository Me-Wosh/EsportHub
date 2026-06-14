using EsportHub.Features.Teams;
using EsportHub.UnitTests.Features;

namespace EsportHub.UnitTests.Features.Teams;

public class GetTeamsQueryHandlerTests : HandlerTestBase
{
    private readonly GetTeamsQueryHandler _handler;

    public GetTeamsQueryHandlerTests()
    {
        _handler = new GetTeamsQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNoTeams_ReturnsEmptyList()
    {
        var result = await _handler.Handle(new GetTeamsQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_GivenExistingTeams_ReturnsAllTeams()
    {
        var tournament = await SeedTournamentAsync();
        await SeedTeamAsync(tournament.Id, "Team Alpha");
        await SeedTeamAsync(tournament.Id, "Team Beta");

        var result = await _handler.Handle(new GetTeamsQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task Handle_GivenTournamentIdFilter_ReturnsOnlyTeamsInTournament()
    {
        var tournament1 = await SeedTournamentAsync("Tournament 1");
        var tournament2 = await SeedTournamentAsync("Tournament 2");
        await SeedTeamAsync(tournament1.Id, "Team Alpha");
        await SeedTeamAsync(tournament2.Id, "Team Beta");

        var result = await _handler.Handle(new GetTeamsQuery(tournament1.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Team Alpha", result.Value.Single().Name);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(new GetTeamsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTeamWithPlayers_ReturnsTeamWithPlayersIncluded()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addHandler = new AddPlayerCommandHandler(Context);
        await addHandler.Handle(new AddPlayerCommand(team.Id, "Player One"), CancellationToken.None);

        var result = await _handler.Handle(new GetTeamsQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Single().Players);
    }
}
