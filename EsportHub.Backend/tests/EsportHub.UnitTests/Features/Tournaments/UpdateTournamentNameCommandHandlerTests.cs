using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class UpdateTournamentNameCommandHandlerTests : HandlerTestBase
{
    private readonly UpdateTournamentNameCommandHandler _handler;

    public UpdateTournamentNameCommandHandlerTests()
    {
        _handler = new UpdateTournamentNameCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new UpdateTournamentNameCommand(Guid.NewGuid(), "New Name"),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenValidName_ReturnsUpdatedTournament()
    {
        var tournament = await SeedTournamentAsync("ESL Pro League");

        var result = await _handler.Handle(
            new UpdateTournamentNameCommand(tournament.Id, "BLAST Premier"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("BLAST Premier", result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenValidName_PersistsUpdatedName()
    {
        var tournament = await SeedTournamentAsync("ESL Pro League");

        await _handler.Handle(
            new UpdateTournamentNameCommand(tournament.Id, "BLAST Premier"),
            CancellationToken.None);

        var getHandler = new GetTournamentQueryHandler(Context);
        var queryResult = await getHandler.Handle(
            new GetTournamentQuery(tournament.Id),
            CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        Assert.Equal("BLAST Premier", queryResult.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenEmptyName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new UpdateTournamentNameCommand(tournament.Id, ""),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var longName = new string('A', TournamentConstraints.NameMaxLength + 1);

        var result = await _handler.Handle(
            new UpdateTournamentNameCommand(tournament.Id, longName),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
