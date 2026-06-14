using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class ResolveGroupStageMatchCommandHandlerTests : HandlerTestBase
{
    private readonly ResolveGroupStageMatchCommandHandler _handler;

    public ResolveGroupStageMatchCommandHandlerTests()
    {
        _handler = new ResolveGroupStageMatchCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new ResolveGroupStageMatchCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2, 1),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentWithoutGroupStage_ReturnsNotFound()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new ResolveGroupStageMatchCommand(tournament.Id, Guid.NewGuid(), Guid.NewGuid(), 2, 1),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenNonExistentGroupId_ReturnsNotFound()
    {
        var (tournament, _) = await SeedGroupStageAsync();

        var result = await _handler.Handle(
            new ResolveGroupStageMatchCommand(tournament.Id, Guid.NewGuid(), Guid.NewGuid(), 2, 1),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenNonExistentMatchId_ReturnsNotFound()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new ResolveGroupStageMatchCommand(tournament.Id, group.Id, Guid.NewGuid(), 2, 1),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenGroupStageIsClosed_ReturnsInvalid()
    {
        var (tournament, groupStage) = await SeedClosedGroupStageAsync();
        var group = groupStage.Groups.First();
        var matchId = Context.GroupStageMatches.First(m => m.GroupId == group.Id).Id;

        var result = await _handler.Handle(
            new ResolveGroupStageMatchCommand(tournament.Id, group.Id, matchId, 2, 1),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
