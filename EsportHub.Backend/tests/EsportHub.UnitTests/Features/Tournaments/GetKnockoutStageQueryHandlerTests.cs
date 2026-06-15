using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class GetKnockoutStageQueryHandlerTests : HandlerTestBase
{
    private readonly GetKnockoutStageQueryHandler _handler;

    public GetKnockoutStageQueryHandlerTests()
    {
        _handler = new GetKnockoutStageQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new GetKnockoutStageQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentWithoutKnockoutStage_ReturnsNotFound()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new GetKnockoutStageQuery(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentWithKnockoutStage_ReturnsKnockoutStage()
    {
        var (tournament, knockoutStage) = await SeedKnockoutStageAsync();

        var result = await _handler.Handle(
            new GetKnockoutStageQuery(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(knockoutStage.Id, result.Value.Id);
        Assert.False(result.Value.IsClosed);
    }

    [Fact]
    public async Task Handle_GivenKnockoutStageWithQuarterFinals_ReturnsQuarterFinalMatches()
    {
        var (tournament, _) = await SeedKnockoutStageAsync();

        var result = await _handler.Handle(
            new GetKnockoutStageQuery(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TournamentConstraints.QuarterFinalMatchesCount, result.Value.Matches.Count());
        Assert.All(result.Value.Matches, m => Assert.Equal(KnockoutStageRound.QuarterFinals, m.Round));
        Assert.All(result.Value.Matches, m => Assert.False(m.IsResolved));
    }

    [Fact]
    public async Task Handle_GivenKnockoutStageWithQuarterFinals_ReturnsMatchesWithTeamNames()
    {
        var (tournament, _) = await SeedKnockoutStageAsync();

        var result = await _handler.Handle(
            new GetKnockoutStageQuery(tournament.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value.Matches, m => Assert.NotEmpty(m.Team1Name));
        Assert.All(result.Value.Matches, m => Assert.NotEmpty(m.Team2Name));
    }
}
