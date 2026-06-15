using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using EsportHub.Features.Teams;

namespace EsportHub.UnitTests.Features.Teams;

public class CreateTeamCommandHandlerTests : HandlerTestBase
{
    private readonly CreateTeamCommandHandler _handler;

    public CreateTeamCommandHandlerTests()
    {
        _handler = new CreateTeamCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTournamentId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new CreateTeamCommand(Guid.NewGuid(), "Team Alpha"),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenValidNameAndTournament_ReturnsCreatedTeam()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new CreateTeamCommand(tournament.Id, "Team Alpha"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Team Alpha", result.Value.Name);
        Assert.Equal(tournament.Id, result.Value.TournamentId);
    }

    [Fact]
    public async Task Handle_GivenValidName_PersistsTeamToDatabase()
    {
        var tournament = await SeedTournamentAsync();

        await _handler.Handle(
            new CreateTeamCommand(tournament.Id, "Team Alpha"),
            CancellationToken.None);

        Assert.Equal(1, Context.Teams.Count());
    }

    [Fact]
    public async Task Handle_GivenEmptyName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();

        var result = await _handler.Handle(
            new CreateTeamCommand(tournament.Id, ""),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var longName = new string('A', TeamConstraints.NameMaxLength + 1);

        var result = await _handler.Handle(
            new CreateTeamCommand(tournament.Id, longName),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenDuplicateTeamName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        await _handler.Handle(new CreateTeamCommand(tournament.Id, "Team Alpha"), CancellationToken.None);

        var result = await _handler.Handle(
            new CreateTeamCommand(tournament.Id, "Team Alpha"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenMaxTeamsAlreadyReached_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        for (var i = 0; i < TournamentConstraints.TeamsRequiredCount; i++)
            await _handler.Handle(new CreateTeamCommand(tournament.Id, $"Team {i + 1}"), CancellationToken.None);

        var result = await _handler.Handle(
            new CreateTeamCommand(tournament.Id, "Extra Team"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
