using EsportHub.Domain.Teams;
using EsportHub.Features.Teams;
using EsportHub.UnitTests.Features;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.UnitTests.Features.Teams;

public class UpdateTeamNameCommandHandlerTests : HandlerTestBase
{
    private readonly UpdateTeamNameCommandHandler _handler;

    public UpdateTeamNameCommandHandlerTests()
    {
        _handler = new UpdateTeamNameCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentTeamId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new UpdateTeamNameCommand(Guid.NewGuid(), "New Name"),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenTournamentNotInPreparation_ReturnsInvalid()
    {
        var (_, groupStage) = await SeedGroupStageAsync();
        var teamId = groupStage.Groups.First().Teams.First().Id;

        var result = await _handler.Handle(
            new UpdateTeamNameCommand(teamId, "New Name"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenDuplicateTeamName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        await SeedTeamAsync(tournament.Id, "Team Alpha");
        var team = await SeedTeamAsync(tournament.Id, "Team Beta");

        var result = await _handler.Handle(
            new UpdateTeamNameCommand(team.Id, "Team Alpha"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenValidName_ReturnsUpdatedTeam()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id, "Team Alpha");

        var result = await _handler.Handle(
            new UpdateTeamNameCommand(team.Id, "Team Omega"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Team Omega", result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenValidName_PersistsUpdatedName()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id, "Team Alpha");

        await _handler.Handle(
            new UpdateTeamNameCommand(team.Id, "Team Omega"),
            CancellationToken.None);

        var updatedTeam = await Context.Teams.SingleAsync(t => t.Id == team.Id);
        Assert.Equal("Team Omega", updatedTeam.Name);
    }

    [Fact]
    public async Task Handle_GivenEmptyName_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        var result = await _handler.Handle(
            new UpdateTeamNameCommand(team.Id, ""),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var longName = new string('A', TeamConstraints.NameMaxLength + 1);

        var result = await _handler.Handle(
            new UpdateTeamNameCommand(team.Id, longName),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
