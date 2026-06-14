using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class GetGroupQueryHandlerTests : HandlerTestBase
{
    private readonly GetGroupQueryHandler _handler;

    public GetGroupQueryHandlerTests()
    {
        _handler = new GetGroupQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentGroupId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new GetGroupQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenExistingGroup_ReturnsGroup()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new GetGroupQuery(tournament.Id, group.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(group.Id, result.Value.Id);
        Assert.Equal(group.Name, result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenExistingGroup_ReturnsGroupWithStandings()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new GetGroupQuery(tournament.Id, group.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(GroupConstraints.TeamsRequiredCount, result.Value.Standings.Count());
    }
}
