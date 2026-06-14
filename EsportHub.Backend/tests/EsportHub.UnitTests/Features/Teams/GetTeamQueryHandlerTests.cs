using EsportHub.Features.Teams;
using EsportHub.UnitTests.Features;

namespace EsportHub.UnitTests.Features.Teams;

public class GetTeamQueryHandlerTests : HandlerTestBase
{
    private readonly GetTeamQueryHandler _handler;

    public GetTeamQueryHandlerTests()
    {
        _handler = new GetTeamQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTeamId_ReturnsNotFound()
    {
        var result = await _handler.Handle(new GetTeamQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenExistingTeam_ReturnsTeam()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id, "Team Alpha");

        var result = await _handler.Handle(new GetTeamQuery(team.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(team.Id, result.Value.Id);
        Assert.Equal("Team Alpha", result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenExistingTeam_ReturnsCorrectTournamentName()
    {
        var tournament = await SeedTournamentAsync("ESL Pro League");
        var team = await SeedTeamAsync(tournament.Id);

        var result = await _handler.Handle(new GetTeamQuery(team.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ESL Pro League", result.Value.TournamentName);
    }

    [Fact]
    public async Task Handle_GivenTeamWithPlayers_ReturnsPlayersIncluded()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addHandler = new AddPlayerCommandHandler(Context);
        await addHandler.Handle(new AddPlayerCommand(team.Id, "Player One"), CancellationToken.None);
        await addHandler.Handle(new AddPlayerCommand(team.Id, "Player Two"), CancellationToken.None);

        var result = await _handler.Handle(new GetTeamQuery(team.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Players.Count());
    }
}
