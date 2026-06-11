using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;

namespace EsportHub.UnitTests.Domain.Tournaments;

public class GroupTests
{
    [Fact]
    public void UpdateName_GivenValidName_UpdatesName()
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;

        var result = group.UpdateName("Group B");

        Assert.True(result.IsSuccess);
        Assert.Equal("Group B", group.Name);
    }

    [Fact]
    public void UpdateName_GivenEmptyName_ReturnsInvalidAndDoesNotChangeName()
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;

        var result = group.UpdateName("");

        Assert.True(result.IsInvalid());
        Assert.Equal("Group A", group.Name);
    }

    [Fact]
    public void UpdateName_GivenNameWithLeadingAndTrailingWhitespace_TrimsAndUpdatesName()
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;

        var result = group.UpdateName("  Group B  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Group B", group.Name);
    }

    [Fact]
    public void UpdateName_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;
        var longName = new string('A', GroupConstraints.NameMaxLength + 1);

        var result = group.UpdateName(longName);

        Assert.True(result.IsInvalid());
        Assert.Equal("Group A", group.Name);
    }

    [Fact]
    public void AddTeam_GivenValidTeam_AddsTeamToGroup()
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;

        var result = group.AddTeam(team);

        Assert.True(result.IsSuccess);
        Assert.Single(group.Teams);
    }

    [Fact]
    public void AddTeam_GivenDuplicateTeam_ReturnsInvalidAndDoesNotAddDuplicate()
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;
        var team = Team.Create("Team Alpha", Guid.NewGuid()).Value;
        group.AddTeam(team);

        var result = group.AddTeam(team);

        Assert.True(result.IsInvalid());
        Assert.Single(group.Teams);
    }

    [Fact]
    public void AddTeam_GivenGroupAtMaxCapacity_ReturnsInvalid()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);

        var result = group.AddTeam(Team.Create("Extra Team", Guid.NewGuid()).Value);

        Assert.True(result.IsInvalid());
        Assert.Equal(GroupConstraints.TeamsRequiredCount, group.Teams.Count);
    }

    [Fact]
    public void InitializeMatches_GivenFourTeams_CreatesRoundRobinMatchesAndStandings()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);

        var result = group.InitializeMatches();

        var expectedMatchCount = GroupConstraints.TeamsRequiredCount * (GroupConstraints.TeamsRequiredCount - 1) / 2;
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedMatchCount, group.Matches.Count);
        Assert.Equal(GroupConstraints.TeamsRequiredCount, group.Standings.Count);
    }

    [Fact]
    public void InitializeMatches_GivenNotEnoughTeams_ReturnsInvalidAndCreatesNoMatches()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount - 1);

        var result = group.InitializeMatches();

        Assert.True(result.IsInvalid());
        Assert.Empty(group.Matches);
    }

    [Fact]
    public void InitializeMatches_GivenAlreadyInitialized_ReturnsInvalid()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);
        group.InitializeMatches();

        var result = group.InitializeMatches();

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void ResolveMatch_GivenValidMatchAndScores_ResolvesMatchAndUpdatesStandings()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);
        group.InitializeMatches();
        var match = group.Matches.First();

        var result = group.ResolveMatch(match.Id, 2, 1);

        Assert.True(result.IsSuccess);
        Assert.True(match.IsResolved);
        Assert.Equal(2, match.Team1Score);
        Assert.Equal(1, match.Team2Score);
    }

    [Fact]
    public void ResolveMatch_GivenNonExistentMatchId_ReturnsNotFound()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);
        group.InitializeMatches();

        var result = group.ResolveMatch(Guid.NewGuid(), 2, 1);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public void ResolveMatch_GivenDrawScores_ReturnsInvalidAndDoesNotResolveMatch()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);
        group.InitializeMatches();
        var match = group.Matches.First();

        var result = group.ResolveMatch(match.Id, 1, 1);

        Assert.True(result.IsInvalid());
        Assert.False(match.IsResolved);
    }

    [Fact]
    public void GetQualifiedTeams_GivenAllMatchesResolved_ReturnsTopTwoTeams()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);
        group.InitializeMatches();
        foreach (var match in group.Matches)
            group.ResolveMatch(match.Id, 2, 1);

        var qualifiedTeams = group.GetQualifiedTeams();

        Assert.Equal(GroupConstraints.QualifiedTeamsCount, qualifiedTeams.Count);
    }

    [Fact]
    public void RecalculateStandings_GivenTeamWinsAllMatches_PlacesTeamFirst()
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;
        var teams = Enumerable.Range(1, GroupConstraints.TeamsRequiredCount)
            .Select(i => Team.Create($"Team {i}", Guid.NewGuid()).Value)
            .ToList();

        foreach (var team in teams)
            group.AddTeam(team);
        group.InitializeMatches();

        foreach (var match in group.Matches.ToList())
        {
            if (match.Team1Id == teams[0].Id)
                group.ResolveMatch(match.Id, 2, 1);
            else if (match.Team2Id == teams[0].Id)
                group.ResolveMatch(match.Id, 1, 2);
        }

        foreach (var match in group.Matches.Where(m => !m.IsResolved).ToList())
            group.ResolveMatch(match.Id, 2, 1);

        var firstPlace = group.Standings.Single(s => s.Position == 1);
        Assert.Equal(teams[0].Id, firstPlace.TeamId);
        Assert.Equal(GroupConstraints.TeamsRequiredCount - 1, firstPlace.Wins);
        Assert.Equal(0, firstPlace.Losses);
    }

    [Fact]
    public void RecalculateStandings_GivenAllMatchesResolved_AllTeamsHaveCorrectGamesPlayed()
    {
        var group = CreateGroupWithTeams(GroupConstraints.TeamsRequiredCount);
        group.InitializeMatches();

        foreach (var match in group.Matches)
            group.ResolveMatch(match.Id, 2, 1);

        var expectedGamesPerTeam = GroupConstraints.TeamsRequiredCount - 1;
        Assert.All(group.Standings, s => Assert.Equal(expectedGamesPerTeam, s.GamesPlayed));
    }

    private static Group CreateGroupWithTeams(int teamCount)
    {
        var group = Group.Create("Group A", Guid.NewGuid()).Value;
        for (var i = 0; i < teamCount; i++)
            group.AddTeam(Team.Create($"Team {i + 1}", Guid.NewGuid()).Value);
        return group;
    }
}
