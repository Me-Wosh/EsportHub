using EsportHub.Features.Tournaments;
using EsportHub.UnitTests.Features;

namespace EsportHub.UnitTests.Features.Tournaments;

public class GetTournamentQueryHandlerTests : HandlerTestBase
{
    private readonly GetTournamentQueryHandler _handler;

    public GetTournamentQueryHandlerTests()
    {
        _handler = new GetTournamentQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(new GetTournamentQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenExistingId_ReturnsTournament()
    {
        var tournament = await SeedTournamentAsync("ESL Pro League");

        var result = await _handler.Handle(new GetTournamentQuery(tournament.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(tournament.Id, result.Value.Id);
        Assert.Equal("ESL Pro League", result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenExistingTournamentWithoutKnockoutStage_ReturnsNullWinner()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(new GetTournamentQuery(tournament.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Winner);
    }
}
