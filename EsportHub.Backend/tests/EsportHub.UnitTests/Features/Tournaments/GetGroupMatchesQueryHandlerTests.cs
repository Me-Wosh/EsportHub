using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class GetGroupMatchesQueryHandlerTests : HandlerTestBase
{
    private readonly GetGroupMatchesQueryHandler _handler;

    public GetGroupMatchesQueryHandlerTests()
    {
        _handler = new GetGroupMatchesQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentGroupId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new GetGroupMatchesQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenExistingGroup_ReturnsMatches()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new GetGroupMatchesQuery(tournament.Id, group.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task Handle_GivenGroupOfFourTeams_ReturnsAllRoundRobinMatches()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        // 4 teams -> C(4,2) = 6 round-robin matches
        var expectedMatchCount = 4 * 3 / 2;

        var result = await _handler.Handle(
            new GetGroupMatchesQuery(tournament.Id, group.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedMatchCount, result.Value.Count());
    }

    [Fact]
    public async Task Handle_GivenNewGroupStage_ReturnsMatchesWithNoScores()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new GetGroupMatchesQuery(tournament.Id, group.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value, m => Assert.False(m.IsResolved));
    }
}
