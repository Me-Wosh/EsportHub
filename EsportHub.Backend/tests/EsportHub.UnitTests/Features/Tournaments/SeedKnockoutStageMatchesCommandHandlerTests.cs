using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class SeedKnockoutStageMatchesCommandHandlerTests : HandlerTestBase
{
    private readonly SeedKnockoutStageMatchesCommandHandler _handler;

    public SeedKnockoutStageMatchesCommandHandlerTests()
    {
        _handler = new SeedKnockoutStageMatchesCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new SeedKnockoutStageMatchesCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentWithoutKnockoutStage_ReturnsNotFound()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new SeedKnockoutStageMatchesCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenKnockoutStageWithUnresolvedMatches_SeedsAllMatchesAndClosesStage()
    {
        var (tournament, _) = await SeedKnockoutStageAsync();

        var result = await _handler.Handle(
            new SeedKnockoutStageMatchesCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsClosed);
        Assert.All(result.Value.Matches, m => Assert.True(m.IsResolved));
    }

    [Fact]
    public async Task Handle_GivenKnockoutStage_SeedsFullBracketWithAllRounds()
    {
        var (tournament, _) = await SeedKnockoutStageAsync();

        var result = await _handler.Handle(
            new SeedKnockoutStageMatchesCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var totalExpectedMatches = TournamentConstraints.QuarterFinalMatchesCount
            + TournamentConstraints.SemiFinalMatchesCount
            + 1;

        Assert.Equal(totalExpectedMatches, result.Value.Matches.Count());
        Assert.Equal(TournamentConstraints.QuarterFinalMatchesCount,
            result.Value.Matches.Count(m => m.Round == KnockoutStageRound.QuarterFinals));
        Assert.Equal(TournamentConstraints.SemiFinalMatchesCount,
            result.Value.Matches.Count(m => m.Round == KnockoutStageRound.SemiFinals));
        Assert.Single(result.Value.Matches, m => m.Round == KnockoutStageRound.Final);
    }

    [Fact]
    public async Task Handle_GivenKnockoutStageAlreadyClosed_ReturnsInvalid()
    {
        var (tournament, _) = await SeedKnockoutStageAsync();

        await _handler.Handle(
            new SeedKnockoutStageMatchesCommand(tournament.Id),
            CancellationToken.None);

        var result = await _handler.Handle(
            new SeedKnockoutStageMatchesCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenPartiallyResolvedKnockoutStage_SeedsRemainingMatches()
    {
        var (tournament, knockoutStage) = await SeedKnockoutStageAsync();

        var firstMatchId = knockoutStage.Matches.First().Id;
        await new ResolveKnockoutStageMatchCommandHandler(Context)
            .Handle(new ResolveKnockoutStageMatchCommand(tournament.Id, firstMatchId, 2, 1), CancellationToken.None);

        var result = await _handler.Handle(
            new SeedKnockoutStageMatchesCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsClosed);
        Assert.All(result.Value.Matches, m => Assert.True(m.IsResolved));
    }
}
