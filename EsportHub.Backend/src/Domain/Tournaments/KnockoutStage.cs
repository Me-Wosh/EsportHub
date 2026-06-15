using EsportHub.Domain.Matches;

namespace EsportHub.Domain.Tournaments;

public class KnockoutStage : BaseEntity
{
    private readonly List<KnockoutStageMatch> _matches = [];
    private readonly Random _random = new();

    private KnockoutStage() { }

    public bool IsClosed { get; private set; }

    public Guid TournamentId { get; private set; }
    public Tournament Tournament { get; private set; } = null!;

    public IReadOnlyCollection<KnockoutStageMatch> Matches => _matches;

    internal static Result<KnockoutStage> Create(Guid tournamentId)
    {
        var knockoutStage = new KnockoutStage();

        return Result.Success()
            .Bind(_ => knockoutStage.UpdateTournamentId(tournamentId));
    }

    internal Result InitializeQuarterFinals(List<GroupTeamStanding> qualifiedTeams)
    {
        if (_matches.Count > 0)
            return Result.Invalid(new ValidationError("Knockout matches are already initialized."));

        if (qualifiedTeams.Count != TournamentConstraints.QualifiedTeamsCount)
        {
            return Result.Invalid(
                new ValidationError(
                    $"Knockout stage requires exactly {TournamentConstraints.QualifiedTeamsCount} teams."));
        }

        var pairings = BuildQuarterFinalPairings(qualifiedTeams);

        foreach (var (index, pair) in pairings.Index())
        {
            var side = index < TournamentConstraints.SemiFinalMatchesCount
                ? KnockoutStageSide.Left
                : KnockoutStageSide.Right;

            var knockoutStageMatch = KnockoutStageMatch.Create(
                this,
                KnockoutStageRound.QuarterFinals,
                side,
                pair.Team1.TeamId,
                pair.Team2.TeamId);

            if (!knockoutStageMatch.IsSuccess)
                return knockoutStageMatch.Map();

            _matches.Add(knockoutStageMatch);
        }

        return Result.Success();
    }

    internal Result<KnockoutStageMatch> ResolveMatch(KnockoutStageMatch match, int team1Score, int team2Score)
    {
        if (IsClosed)
            return Result.Invalid(new ValidationError("Cannot resolve match after knockout stage is closed."));

        var setScoresResult = match.SetScores(team1Score, team2Score);
        if (!setScoresResult.IsSuccess)
            return setScoresResult;

        var updateBracketResult = AdvanceBracket();
        if (!updateBracketResult.IsSuccess)
            return updateBracketResult;

        return Result.Success(match);
    }

    private Result<KnockoutStage> UpdateTournamentId(Guid tournamentId)
    {
        if (tournamentId == Guid.Empty)
            return Result.Invalid(new ValidationError("Knockout stage must belong to a tournament."));

        TournamentId = tournamentId;
        return this;
    }

    private List<KnockoutPair> BuildQuarterFinalPairings(List<GroupTeamStanding> participants)
    {
        var groupings = participants
            .GroupBy(p => p.Group.Id)
            .Select(g => g.ToArray())
            .ToArray();

        _random.Shuffle(groupings);

        foreach (var group in groupings)
            _random.Shuffle(group);

        return
        [
            new KnockoutPair(groupings[0][0], groupings[1][0]),
            new KnockoutPair(groupings[0][1], groupings[1][1]),
            new KnockoutPair(groupings[2][0], groupings[3][0]),
            new KnockoutPair(groupings[2][1], groupings[3][1]),
        ];
    }

    private Result AdvanceBracket()
    {
        var finalMatch = _matches.SingleOrDefault(m => m.Round == KnockoutStageRound.Final);

        if (finalMatch is not null)
        {
            if (finalMatch.IsResolved)
                IsClosed = true;

            return Result.Success();
        }

        if (_matches.Any(m => m.Round == KnockoutStageRound.SemiFinals))
            return CreateFinal();

        return CreateSemiFinals();
    }

    private Result CreateSemiFinals()
    {
        if (_matches.Any(m => m.Round == KnockoutStageRound.SemiFinals))
            return Result.Success();

        var quarterFinals = _matches.Where(m => m.Round == KnockoutStageRound.QuarterFinals);
        if (quarterFinals.Any(match => !match.IsResolved))
            return Result.Success();

        var leftWinners = quarterFinals
            .Where(match => match.Side == KnockoutStageSide.Left)
            .Select(match => match.WinnerTeamId!.Value)
            .ToList();

        var rightWinners = quarterFinals
            .Where(match => match.Side == KnockoutStageSide.Right)
            .Select(match => match.WinnerTeamId!.Value)
            .ToList();

        var createLeftSemiFinalResult = KnockoutStageMatch.Create(
            this,
            KnockoutStageRound.SemiFinals,
            KnockoutStageSide.Left,
            leftWinners[0],
            leftWinners[1]);

        if (!createLeftSemiFinalResult.IsSuccess)
            return createLeftSemiFinalResult.Map();

        var createRightSemiFinalResult = KnockoutStageMatch.Create(
            this,
            KnockoutStageRound.SemiFinals,
            KnockoutStageSide.Right,
            rightWinners[0],
            rightWinners[1]);

        if (!createRightSemiFinalResult.IsSuccess)
            return createRightSemiFinalResult.Map();

        _matches.Add(createLeftSemiFinalResult.Value);
        _matches.Add(createRightSemiFinalResult.Value);

        return Result.Success();
    }

    private Result CreateFinal()
    {
        var semiFinals = _matches.Where(m => m.Round == KnockoutStageRound.SemiFinals);
        if (semiFinals.Any(match => !match.IsResolved))
            return Result.Success();

        if (_matches.Any(m => m.Round == KnockoutStageRound.Final))
            return Result.Success();

        var winners = semiFinals
            .Select(match => match.WinnerTeamId!.Value)
            .ToList();

        var createFinalResult = KnockoutStageMatch.Create(
            this,
            KnockoutStageRound.Final,
            null,
            winners[0],
            winners[1]);

        if (!createFinalResult.IsSuccess)
            return createFinalResult.Map();

        _matches.Add(createFinalResult.Value);
        return Result.Success();
    }

    private sealed record KnockoutPair(GroupTeamStanding Team1, GroupTeamStanding Team2);
}
