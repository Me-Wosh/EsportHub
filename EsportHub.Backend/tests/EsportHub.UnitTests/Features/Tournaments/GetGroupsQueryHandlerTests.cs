using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class GetGroupsQueryHandlerTests : GroupStageHandlerTestBase
{
    private readonly GetGroupsQueryHandler _handler;

    public GetGroupsQueryHandlerTests()
    {
        _handler = new GetGroupsQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(new GetGroupsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentWithNoGroupStage_ReturnsEmptyList()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(new GetGroupsQuery(tournament.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_GivenTournamentWithGroupStage_ReturnsGroups()
    {
        var (tournament, _) = await SeedGroupStageAsync();

        var result = await _handler.Handle(new GetGroupsQuery(tournament.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TournamentConstraints.GroupsRequiredCount, result.Value.Count());
    }

    [Fact]
    public async Task Handle_GivenGroupStage_ReturnsGroupsWithStandings()
    {
        var (tournament, _) = await SeedGroupStageAsync();

        var result = await _handler.Handle(new GetGroupsQuery(tournament.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value, g => Assert.Equal(GroupConstraints.TeamsRequiredCount, g.Standings.Count()));
    }
}
