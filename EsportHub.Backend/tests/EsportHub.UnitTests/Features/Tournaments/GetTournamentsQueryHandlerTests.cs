using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class GetTournamentsQueryHandlerTests : HandlerTestBase
{
    private readonly GetTournamentsQueryHandler _handler;

    public GetTournamentsQueryHandlerTests()
    {
        _handler = new GetTournamentsQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNoTournaments_ReturnsEmptyList()
    {
        var result = await _handler.Handle(new GetTournamentsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_GivenExistingTournaments_ReturnsAllTournaments()
    {
        await SeedTournamentAsync("ESL Pro League");
        await SeedTournamentAsync("BLAST Premier");

        var result = await _handler.Handle(new GetTournamentsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task Handle_GivenExistingTournament_ReturnsCorrectName()
    {
        await SeedTournamentAsync("ESL Pro League");

        var result = await _handler.Handle(new GetTournamentsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ESL Pro League", result.Value.Single().Name);
    }

    [Fact]
    public async Task Handle_GivenTournamentWithNoKnockoutStage_ReturnsNullWinner()
    {
        await SeedTournamentAsync();

        var result = await _handler.Handle(new GetTournamentsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Single().Winner);
    }
}
