using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class CloseGroupStageCommandHandlerTests : HandlerTestBase
{
    private readonly CloseGroupStageCommandHandler _handler;

    public CloseGroupStageCommandHandlerTests()
    {
        _handler = new CloseGroupStageCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new CloseGroupStageCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentWithoutGroupStage_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new CloseGroupStageCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenGroupStageWithUnresolvedMatches_ReturnsInvalid()
    {
        var (tournament, _) = await SeedGroupStageAsync();

        var result = await _handler.Handle(
            new CloseGroupStageCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenGroupStageAlreadyClosed_ReturnsInvalid()
    {
        var (tournament, _) = await SeedClosedGroupStageAsync();

        var result = await _handler.Handle(
            new CloseGroupStageCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
