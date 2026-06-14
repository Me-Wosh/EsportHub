using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;
using EsportHub.UnitTests.Features;

namespace EsportHub.UnitTests.Features.Tournaments;

public class CreateTournamentCommandHandlerTests : HandlerTestBase
{
    private readonly CreateTournamentCommandHandler _handler;

    public CreateTournamentCommandHandlerTests()
    {
        _handler = new CreateTournamentCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenValidName_ReturnsCreatedTournamentWithInPreparationStatus()
    {
        var result = await _handler.Handle(
            new CreateTournamentCommand("ESL Pro League"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ESL Pro League", result.Value.Name);
        Assert.Equal(TournamentStatus.InPreparation, result.Value.Status);
    }

    [Fact]
    public async Task Handle_GivenValidName_PersistsTournamentToDatabase()
    {
        var result = await _handler.Handle(
            new CreateTournamentCommand("ESL Pro League"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, Context.Tournaments.Count());
    }

    [Fact]
    public async Task Handle_GivenEmptyName_ReturnsInvalid()
    {
        var result = await _handler.Handle(
            new CreateTournamentCommand(""),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var longName = new string('A', TournamentConstraints.NameMaxLength + 1);

        var result = await _handler.Handle(
            new CreateTournamentCommand(longName),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenWhitespaceOnlyName_ReturnsInvalid()
    {
        var result = await _handler.Handle(
            new CreateTournamentCommand("   "),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameWithLeadingAndTrailingWhitespace_TrimsName()
    {
        var result = await _handler.Handle(
            new CreateTournamentCommand("  ESL Pro League  "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ESL Pro League", result.Value.Name);
    }
}
