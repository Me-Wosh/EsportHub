using EsportHub.Domain.Matches;
using EsportHub.Domain.Tournaments;

namespace EsportHub.UnitTests.Domain.Matches;

public class MatchTests
{
    [Fact]
    public void Create_GivenValidQuarterFinalMatchWithSide_Succeeds()
    {
        var (team1Id, team2Id) = (Guid.NewGuid(), Guid.NewGuid());

        var result = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.QuarterFinals, KnockoutStageSide.Left, team1Id, team2Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(KnockoutStageRound.QuarterFinals, result.Value.Round);
        Assert.Equal(KnockoutStageSide.Left, result.Value.Side);
    }

    [Fact]
    public void Create_GivenFinalRoundWithoutSide_Succeeds()
    {
        var (team1Id, team2Id) = (Guid.NewGuid(), Guid.NewGuid());

        var result = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.Final, null, team1Id, team2Id);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Side);
    }

    [Fact]
    public void Create_GivenFinalRoundWithSideAssigned_ReturnsInvalid()
    {
        var (team1Id, team2Id) = (Guid.NewGuid(), Guid.NewGuid());

        var result = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.Final, KnockoutStageSide.Left, team1Id, team2Id);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenNonFinalRoundWithoutSide_ReturnsInvalid()
    {
        var (team1Id, team2Id) = (Guid.NewGuid(), Guid.NewGuid());

        var result = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.SemiFinals, null, team1Id, team2Id);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenSameTeamIds_ReturnsInvalid()
    {
        var teamId = Guid.NewGuid();

        var result = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.QuarterFinals, KnockoutStageSide.Left, teamId, teamId);

        Assert.True(result.IsInvalid());
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111")]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000")]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    public void Create_GivenEmptyTeamId_ReturnsInvalid(Guid team1Id, Guid team2Id)
    {
        var result = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.QuarterFinals, KnockoutStageSide.Left, team1Id, team2Id);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Create_GivenNullKnockoutStage_ReturnsInvalid()
    {
        var (team1Id, team2Id) = (Guid.NewGuid(), Guid.NewGuid());

        var result = KnockoutStageMatch.Create(
            null!, KnockoutStageRound.QuarterFinals, KnockoutStageSide.Left, team1Id, team2Id);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void SetScores_GivenValidDifferentScores_SetsScoresAndResolvesMatch()
    {
        var match = CreateQuarterFinalMatch();

        var result = match.SetScores(2, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, match.Team1Score);
        Assert.Equal(1, match.Team2Score);
        Assert.True(match.IsResolved);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(2, -1)]
    [InlineData(-1, -1)]
    public void SetScores_GivenNegativeTeamScores_ReturnsInvalidAndDoesNotResolveMatch(int team1Score, int team2Score)
    {
        var match = CreateQuarterFinalMatch();

        var result = match.SetScores(team1Score, team2Score);

        Assert.True(result.IsInvalid());
        Assert.False(match.IsResolved);
    }

    [Fact]
    public void SetScores_GivenEqualScores_ReturnsInvalidAndDoesNotResolveMatch()
    {
        var match = CreateQuarterFinalMatch();

        var result = match.SetScores(2, 2);

        Assert.True(result.IsInvalid());
        Assert.False(match.IsResolved);
    }

    [Fact]
    public void SetScores_GivenMatchAlreadyResolved_ReturnsInvalidAndDoesNotChangeScores()
    {
        var match = CreateQuarterFinalMatch();
        match.SetScores(2, 1);

        var result = match.SetScores(3, 1);

        Assert.True(result.IsInvalid());
        Assert.Equal(2, match.Team1Score);
        Assert.Equal(1, match.Team2Score);
    }

    [Fact]
    public void WinnerTeamId_GivenOneTeamScoreHigherThanOtherTeam_ReturnsTeamIdWithHigherScore()
    {
        var team1Id = Guid.NewGuid();
        var team2Id = Guid.NewGuid();

        var matchWithTeam1AsWinner = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.QuarterFinals, KnockoutStageSide.Left, team1Id, team2Id).Value;
        matchWithTeam1AsWinner.SetScores(3, 1);
        var matchWithTeam2AsWinner = KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(), KnockoutStageRound.QuarterFinals, KnockoutStageSide.Left, team1Id, team2Id).Value;
        matchWithTeam2AsWinner.SetScores(1, 3);

        Assert.Equal(team1Id, matchWithTeam1AsWinner.WinnerTeamId);
        Assert.Equal(team2Id, matchWithTeam2AsWinner.WinnerTeamId);
    }

    [Fact]
    public void WinnerTeamId_GivenMatchNotResolved_ReturnsNull()
    {
        var match = CreateQuarterFinalMatch();

        Assert.Null(match.WinnerTeamId);
    }

    private static KnockoutStage CreateDummyKnockoutStage() =>
        (KnockoutStage)Activator.CreateInstance(typeof(KnockoutStage), nonPublic: true)!;

    private static KnockoutStageMatch CreateQuarterFinalMatch() =>
        KnockoutStageMatch.Create(
            CreateDummyKnockoutStage(),
            KnockoutStageRound.QuarterFinals,
            KnockoutStageSide.Left,
            Guid.NewGuid(),
            Guid.NewGuid());
}
