using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;

namespace EsportHub.UnitTests.Domain.Tournaments;

public class TournamentTests
{
    [Fact]
    public void Create_GivenValidName_ReturnsSuccessWithStatusInPreparation()
    {
        var result = Tournament.Create("ESL Pro League");

        Assert.True(result.IsSuccess);
        Assert.Equal("ESL Pro League", result.Value.Name);
        Assert.Equal(TournamentStatus.InPreparation, result.Value.Status);
    }

    [Fact]
    public void Create_GivenEmptyName_ReturnsInvalid()
    {
        var result = Tournament.Create("");

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenWhitespaceOnlyName_ReturnsInvalid()
    {
        var result = Tournament.Create("   ");

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var longName = new string('A', TournamentConstraints.NameMaxLength + 1);

        var result = Tournament.Create(longName);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void UpdateName_GivenValidName_UpdatesName()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;

        var result = tournament.UpdateName("BLAST Premier");

        Assert.True(result.IsSuccess);
        Assert.Equal("BLAST Premier", tournament.Name);
    }

    [Fact]
    public void UpdateName_GivenNameWithLeadingAndTrailingWhitespace_TrimsAndUpdatesName()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;

        var result = tournament.UpdateName("  BLAST Premier  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("BLAST Premier", tournament.Name);
    }

    [Fact]
    public void UpdateName_GivenEmptyName_ReturnsInvalidAndDoesNotChangeName()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;

        var result = tournament.UpdateName("");

        Assert.True(result.IsInvalid());
        Assert.Equal("ESL Pro League", tournament.Name);
    }

    [Fact]
    public void Start_GivenValidTeamsAndGroupNames_StartsGroupStageAndSetsStartDate()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;
        var teams = CreateTeamsWithMinRoster(TournamentConstraints.TeamsRequiredCount, tournament.Id);

        var result = tournament.Start(teams, GetDefaultGroupNames());

        Assert.True(result.IsSuccess);
        Assert.Equal(TournamentStatus.GroupStage, tournament.Status);
        Assert.NotNull(tournament.GroupStage);
        Assert.Equal(TournamentConstraints.GroupsRequiredCount, tournament.GroupStage.Groups.Count);
        Assert.NotNull(tournament.StartDate);
    }

    [Fact]
    public void Start_GivenTournamentAlreadyInGroupStage_ReturnsInvalid()
    {
        var tournament = CreateStartedTournament();
        var teams = CreateTeamsWithMinRoster(TournamentConstraints.TeamsRequiredCount, tournament.Id);

        var result = tournament.Start(teams, GetDefaultGroupNames());

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Start_GivenWrongNumberOfTeams_ReturnsInvalidAndKeepsStatusInPreparation()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;
        var teams = CreateTeamsWithMinRoster(TournamentConstraints.TeamsRequiredCount - 1, tournament.Id);

        var result = tournament.Start(teams, GetDefaultGroupNames());

        Assert.True(result.IsInvalid());
        Assert.Equal(TournamentStatus.InPreparation, tournament.Status);
    }

    [Fact]
    public void Start_GivenTeamWithInsufficientRoster_ReturnsInvalidAndKeepsStatusInPreparation()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;
        var teams = CreateTeamsWithMinRoster(TournamentConstraints.TeamsRequiredCount, tournament.Id);
        var understrengthTeam = Team.Create("Understrengh Team", tournament.Id).Value;
        for (var i = 0; i < TeamConstraints.PlayersMinCount - 1; i++)
            understrengthTeam.AddPlayer($"Player {i + 1}");
        teams[^1] = understrengthTeam;

        var result = tournament.Start(teams, GetDefaultGroupNames());

        Assert.True(result.IsInvalid());
        Assert.Equal(TournamentStatus.InPreparation, tournament.Status);
    }

    [Fact]
    public void Start_GivenWrongNumberOfGroupNames_ReturnsInvalidAndKeepsStatusInPreparation()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;
        var teams = CreateTeamsWithMinRoster(TournamentConstraints.TeamsRequiredCount, tournament.Id);
        var tooFewGroupNames = new List<string> { "Group A", "Group B", "Group C" };

        var result = tournament.Start(teams, tooFewGroupNames);

        Assert.True(result.IsInvalid());
        Assert.Equal(TournamentStatus.InPreparation, tournament.Status);
    }

    [Fact]
    public void CloseGroupStage_GivenAllMatchesResolved_StartsKnockoutStageWithQuarterFinals()
    {
        var tournament = CreateStartedTournament();
        ResolveAllGroupMatches(tournament);

        var result = tournament.CloseGroupStage();

        Assert.True(result.IsSuccess);
        Assert.Equal(TournamentStatus.KnockoutStage, tournament.Status);
        Assert.NotNull(tournament.KnockoutStage);
        Assert.Equal(TournamentConstraints.QuarterFinalMatchesCount, tournament.KnockoutStage.Matches.Count);
    }

    [Fact]
    public void CloseGroupStage_GivenWithUnresolvedGroupMatches_ReturnsInvalid()
    {
        var tournament = CreateStartedTournament();

        var result = tournament.CloseGroupStage();

        Assert.True(result.IsInvalid());
        Assert.Equal(TournamentStatus.GroupStage, tournament.Status);
    }

    [Fact]
    public void CloseGroupStage_GivenGroupStageNotInitialized_ReturnsInvalid()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;

        var result = tournament.CloseGroupStage();

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Resolve_GivenFinalResolved_ClosesKnockoutStage()
    {
        var tournament = CreateStartedTournament();
        ResolveAllGroupMatches(tournament);
        tournament.CloseGroupStage();

        var ks = tournament.KnockoutStage!;

        foreach (var match in ks.Matches.ToList())
            ks.ResolveMatch(match.Id, 2, 1);

        Assert.Equal(
            TournamentConstraints.QuarterFinalMatchesCount + TournamentConstraints.SemiFinalMatchesCount,
            ks.Matches.Count);

        foreach (var match in ks.Matches.Where(m => m.Round == KnockoutStageRound.SemiFinals).ToList())
            ks.ResolveMatch(match.Id, 2, 1);

        Assert.Equal(
            TournamentConstraints.QuarterFinalMatchesCount + TournamentConstraints.SemiFinalMatchesCount + 1,
            ks.Matches.Count);

        var finalMatch = ks.Matches.Single(m => m.Round == KnockoutStageRound.Final);
        ks.ResolveMatch(finalMatch.Id, 2, 1);

        Assert.True(ks.IsClosed);
    }

    private static Tournament CreateStartedTournament()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;
        var teams = CreateTeamsWithMinRoster(TournamentConstraints.TeamsRequiredCount, tournament.Id);
        tournament.Start(teams, GetDefaultGroupNames());
        return tournament;
    }

    private static List<Team> CreateTeamsWithMinRoster(int count, Guid tournamentId)
    {
        var teams = new List<Team>();
        for (var i = 0; i < count; i++)
        {
            var team = Team.Create($"Team {i + 1}", tournamentId).Value;
            for (var j = 0; j < TeamConstraints.PlayersMinCount; j++)
                team.AddPlayer($"Player {j + 1}");
            teams.Add(team);
        }
        return teams;
    }

    private static List<string> GetDefaultGroupNames() =>
        ["Group A", "Group B", "Group C", "Group D"];

    private static void ResolveAllGroupMatches(Tournament tournament)
    {
        foreach (var group in tournament.GroupStage!.Groups)
        {
            foreach (var match in group.Matches)
                group.ResolveMatch(match.Id, 2, 1);
        }
    }
}
