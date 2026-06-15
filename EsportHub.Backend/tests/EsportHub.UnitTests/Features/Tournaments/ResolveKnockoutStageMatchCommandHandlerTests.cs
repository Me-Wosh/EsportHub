using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class ResolveKnockoutStageMatchCommandHandlerTests : HandlerTestBase
{
    private readonly ResolveKnockoutStageMatchCommandHandler _handler;

    public ResolveKnockoutStageMatchCommandHandlerTests()
    {
        _handler = new ResolveKnockoutStageMatchCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(Guid.NewGuid(), Guid.NewGuid(), 2, 1),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentWithoutKnockoutStage_ReturnsNotFound()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, Guid.NewGuid(), 2, 1),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenNonExistentMatchId_ReturnsNotFound()
    {
        var (tournament, _) = await SeedKnockoutStageAsync();

        var result = await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, Guid.NewGuid(), 2, 1),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenValidMatchAndScores_ResolvesMatchAndReturnsResult()
    {
        var (tournament, knockoutStage) = await SeedKnockoutStageAsync();
        var matchId = knockoutStage.Matches.First().Id;

        var result = await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, matchId, 2, 1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(matchId, result.Value.Id);
        Assert.True(result.Value.IsResolved);
        Assert.Equal(2, result.Value.Team1Score);
        Assert.Equal(1, result.Value.Team2Score);
    }

    [Fact]
    public async Task Handle_GivenDrawScores_ReturnsInvalid()
    {
        var (tournament, knockoutStage) = await SeedKnockoutStageAsync();
        var matchId = knockoutStage.Matches.First().Id;

        var result = await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, matchId, 2, 2),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenAlreadyResolvedMatch_ReturnsInvalid()
    {
        var (tournament, knockoutStage) = await SeedKnockoutStageAsync();
        var matchId = knockoutStage.Matches.First().Id;

        await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, matchId, 2, 1),
            CancellationToken.None);

        var result = await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, matchId, 3, 1),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenAllQuarterFinalsResolved_CreatesSemiFinalMatches()
    {
        var (tournament, knockoutStage) = await SeedKnockoutStageAsync();

        foreach (var match in knockoutStage.Matches.ToList())
        {
            await _handler.Handle(
                new ResolveKnockoutStageMatchCommand(tournament.Id, match.Id, 2, 1),
                CancellationToken.None);
        }

        var semiFinalCount = Context.KnockoutStageMatches
            .Count(m => m.Round == KnockoutStageRound.SemiFinals
                && m.KnockoutStageId == knockoutStage.Id);

        Assert.Equal(TournamentConstraints.SemiFinalMatchesCount, semiFinalCount);
    }

    [Fact]
    public async Task Handle_GivenKnockoutStageIsClosed_ReturnsInvalid()
    {
        var (tournament, knockoutStage) = await SeedKnockoutStageAsync();

        // Resolve all quarter finals
        foreach (var match in knockoutStage.Matches.ToList())
        {
            await _handler.Handle(
                new ResolveKnockoutStageMatchCommand(tournament.Id, match.Id, 2, 1),
                CancellationToken.None);
        }

        // Resolve semi finals
        var semiFinalIds = Context.KnockoutStageMatches
            .Where(m => m.Round == KnockoutStageRound.SemiFinals && m.KnockoutStageId == knockoutStage.Id)
            .Select(m => m.Id)
            .ToList();

        foreach (var matchId in semiFinalIds)
        {
            await _handler.Handle(
                new ResolveKnockoutStageMatchCommand(tournament.Id, matchId, 2, 1),
                CancellationToken.None);
        }

        // Resolve final
        var finalMatchId = Context.KnockoutStageMatches
            .Single(m => m.Round == KnockoutStageRound.Final && m.KnockoutStageId == knockoutStage.Id)
            .Id;

        await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, finalMatchId, 2, 1),
            CancellationToken.None);

        // Try to resolve any match after stage is closed
        var result = await _handler.Handle(
            new ResolveKnockoutStageMatchCommand(tournament.Id, knockoutStage.Matches.First().Id, 3, 1),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
