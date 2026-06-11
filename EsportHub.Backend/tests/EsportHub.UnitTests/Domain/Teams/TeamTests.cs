using EsportHub.Domain.Teams;

namespace EsportHub.UnitTests.Domain.Teams;

public class TeamTests
{
    [Fact]
    public void Create_GivenValidNameAndTournamentId_Succeeds()
    {
        var tournamentId = Guid.NewGuid();

        var result = Team.Create("Team Alpha", tournamentId);

        Assert.True(result.IsSuccess);
        Assert.Equal("Team Alpha", result.Value.Name);
        Assert.Equal(tournamentId, result.Value.TournamentId);
    }

    [Fact]
    public void Create_GivenEmptyName_ReturnsInvalid()
    {
        var result = Team.Create("", Guid.NewGuid());

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenWhitespaceOnlyName_ReturnsInvalid()
    {
        var result = Team.Create("   ", Guid.NewGuid());

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenNameWithLeadingAndTrailingWhitespace_TrimsName()
    {
        var result = Team.Create("  Team Alpha  ", Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal("Team Alpha", result.Value.Name);
    }

    [Fact]
    public void Create_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var longName = new string('A', TeamConstraints.NameMaxLength + 1);

        var result = Team.Create(longName, Guid.NewGuid());

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenEmptyTournamentId_ReturnsInvalid()
    {
        var result = Team.Create("Team Alpha", Guid.Empty);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void UpdateName_GivenValidName_UpdatesName()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = team.UpdateName("Team Beta");

        Assert.True(result.IsSuccess);
        Assert.Equal("Team Beta", team.Name);
    }

    [Fact]
    public void UpdateName_GivenNameWithLeadingAndTrailingWhitespace_TrimsAndUpdatesName()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = team.UpdateName("  Team Beta  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Team Beta", team.Name);
    }

    [Fact]
    public void UpdateName_GivenEmptyName_ReturnsInvalidAndDoesNotChangeName()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = team.UpdateName("");

        Assert.True(result.IsInvalid());
        Assert.Equal("Team Alpha", team.Name);
    }

    [Fact]
    public void UpdateName_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;
        var longName = new string('A', TeamConstraints.NameMaxLength + 1);

        var result = team.UpdateName(longName);

        Assert.True(result.IsInvalid());
        Assert.Equal("Team Alpha", team.Name);
    }

    [Fact]
    public void AddPlayer_GivenValidName_AddsPlayerToTeam()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = team.AddPlayer("Player One");

        Assert.True(result.IsSuccess);
        Assert.Equal("Player One", result.Value.Name);
        Assert.Single(team.Players);
    }

    [Fact]
    public void AddPlayer_GivenMaxPlayersReached_ReturnsInvalid()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;
        for (var i = 0; i < TeamConstraints.PlayersMaxCount; i++)
            team.AddPlayer($"Player {i + 1}");

        var result = team.AddPlayer("Extra Player");

        Assert.True(result.IsInvalid());
        Assert.Equal(TeamConstraints.PlayersMaxCount, team.Players.Count);
    }

    [Fact]
    public void AddPlayer_GivenEmptyName_ReturnsInvalidAndDoesNotAddPlayer()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = team.AddPlayer("");

        Assert.True(result.IsInvalid());
        Assert.Empty(team.Players);
    }

    [Fact]
    public void UpdatePlayerName_GivenExistingPlayer_UpdatesName()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;
        var player = team.AddPlayer("Player One").Value;

        var result = team.UpdatePlayerName(player.Id, "Player One Updated");

        Assert.True(result.IsSuccess);
        Assert.Equal("Player One Updated", player.Name);
    }

    [Fact]
    public void UpdatePlayerName_GivenNonExistingPlayerId_ReturnsNotFound()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = team.UpdatePlayerName(Guid.NewGuid(), "New Name");

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public void RemovePlayer_GivenExistingPlayer_RemovesPlayer()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;
        var player = team.AddPlayer("Player One").Value;

        var result = team.RemovePlayer(player.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(team.Players);
    }

    [Fact]
    public void RemovePlayer_GivenNonExistingPlayerId_ReturnsNotFound()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = team.RemovePlayer(Guid.NewGuid());

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public void HasMinimumRoster_GivenFewerThanMinimumPlayers_ReturnsFalse()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;
        for (var i = 0; i < TeamConstraints.PlayersMinCount - 1; i++)
            team.AddPlayer($"Player {i + 1}");

        Assert.False(team.HasMinimumRoster);
    }

    [Fact]
    public void HasMinimumRoster_GivenExactlyMinimumPlayers_ReturnsTrue()
    {
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;
        for (var i = 0; i < TeamConstraints.PlayersMinCount; i++)
            team.AddPlayer($"Player {i + 1}");

        Assert.True(team.HasMinimumRoster);
    }
}
