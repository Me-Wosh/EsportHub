using EsportHub.Domain.Matches;
using EsportHub.Domain.Teams;

namespace EsportHub.Domain.Tournaments;

public class Group : BaseEntity
{
    private readonly List<Team> _teams = [];
    private readonly List<GroupStageMatch> _matches = [];
    private readonly List<GroupTeamStanding> _standings = [];

    private Group() { }

    public string Name { get; private set; } = string.Empty;

    public Guid GroupStageId { get; private set; }
    public GroupStage GroupStage { get; private set; } = null!;

    public IReadOnlyCollection<Team> Teams => _teams;
    public IReadOnlyCollection<GroupStageMatch> Matches => _matches;
    public IReadOnlyCollection<GroupTeamStanding> Standings => _standings;

    internal static Result<Group> Create(string name, Guid groupStageId)
    {
        var group = new Group();

        return Result.Success()
            .Bind(_ => group.UpdateName(name))
            .Bind(_ => group.UpdateGroupStage(groupStageId));
    }

    public Result<Group> UpdateName(string name)
    {
        name = name.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError("Group name cannot be empty."));

        if (name.Length > GroupConstraints.NameMaxLength)
            errors.Add(new ValidationError($"Group name cannot exceed {GroupConstraints.NameMaxLength} characters."));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Name = name;
        return this;
    }

    internal Result AddTeam(Team team)
    {
        if (_teams.Count >= GroupConstraints.TeamsRequiredCount)
        {
            return Result.Invalid(
                new ValidationError($"Group cannot contain more than {GroupConstraints.TeamsRequiredCount} teams."));
        }

        if (_teams.Any(t => t.Id == team.Id))
            return Result.Invalid(new ValidationError("Team is already part of the group."));

        _teams.Add(team);
        return Result.Success();
    }

    internal Result InitializeMatches()
    {
        if (_matches.Count > 0)
            return Result.Invalid(new ValidationError("Matches have already been initialized for this group."));

        if (_teams.Count != GroupConstraints.TeamsRequiredCount)
        {
            return Result.Invalid(
                new ValidationError(
                    $"Group must contain exactly {GroupConstraints.TeamsRequiredCount} teams to initialize matches."));
        }

        if (_standings.Count > 0)
            return Result.Invalid(new ValidationError("Standings have already been initialized for this group."));

        foreach (var team in _teams)
        {
            var createStandingResult = GroupTeamStanding.Create(Id, team.Id);
            if (!createStandingResult.IsSuccess)
                return createStandingResult.Map();

            _standings.Add(createStandingResult.Value);
        }

        for (int i = 0; i < _teams.Count; i++)
        {
            for (int j = i + 1; j < _teams.Count; j++)
            {
                var match = GroupStageMatch.Create(Id, _teams[i].Id, _teams[j].Id);
                if (!match.IsSuccess)
                    return match.Map();

                _matches.Add(match);
            }
        }

        return RecalculateStandings();
    }

    internal Result<GroupStageMatch> ResolveMatch(Guid matchId, int team1Score, int team2Score)
    {
        var match = _matches.SingleOrDefault(m => m.Id == matchId);
        if (match is null)
            return Result.NotFound("Match was not found in this group.");

        var setScoresResult = match.SetScores(team1Score, team2Score);
        if (!setScoresResult.IsSuccess)
            return setScoresResult.Map();

        var recalculateStandingsResult = RecalculateStandings();
        if (!recalculateStandingsResult.IsSuccess)
            return recalculateStandingsResult.Map();

        return Result.Success(match);
    }

    internal List<GroupTeamStanding> GetQualifiedTeams()
    {
        return _standings
            .OrderBy(s => s.Position)
            .Take(GroupConstraints.QualifiedTeamsCount)
            .ToList();
    }

    private Result<Group> UpdateGroupStage(Guid groupStageId)
    {
        if (groupStageId == Guid.Empty)
            return Result.Invalid(new ValidationError("Group must belong to a group stage."));

        GroupStageId = groupStageId;
        return this;
    }

    private Result RecalculateStandings()
    {
        var resolvedMatches = _matches.Where(m => m.IsResolved).ToList();
        var teamIds = _teams.Select(t => t.Id).ToList();
        var orderedTeamIds = RankTeamsByTieBreakers(teamIds, resolvedMatches);
        var allStats = BuildStats(teamIds, resolvedMatches);

        for (var index = 0; index < orderedTeamIds.Count; index++)
        {
            var teamId = orderedTeamIds[index];
            var standing = _standings.Single(s => s.TeamId == teamId);
            var stats = allStats[teamId];

            var setStatsResult = standing.SetStats(
                stats.GamesPlayed,
                stats.Wins,
                stats.Losses,
                stats.PointsFor,
                stats.PointsAgainst);

            if (!setStatsResult.IsSuccess)
                return setStatsResult.Map();

            var updatePositionResult = standing.UpdatePosition(index + 1);
            if (!updatePositionResult.IsSuccess)
                return updatePositionResult.Map();
        }

        return Result.Success();
    }

    private static List<Guid> RankTeamsByTieBreakers(List<Guid> teamIds, List<GroupStageMatch> resolvedMatches)
    {
        if (teamIds.Count <= 1)
            return teamIds;

        var stats = BuildStats(teamIds, resolvedMatches);

        var groupsByPrimaryCriteria = teamIds
            .GroupBy(id => new { stats[id].Wins, stats[id].PointDifference })
            .OrderByDescending(group => group.Key.Wins)
            .ThenByDescending(group => group.Key.PointDifference);

        var ranking = new List<Guid>();

        foreach (var tiedGroup in groupsByPrimaryCriteria)
        {
            var tiedTeamIds = tiedGroup.ToList();

            if (tiedTeamIds.Count == 1)
            {
                ranking.Add(tiedTeamIds[0]);
                continue;
            }

            if (tiedTeamIds.Count == 2)
            {
                ranking.AddRange(ResolveTwoTeamTie(tiedTeamIds, resolvedMatches));
                continue;
            }

            ranking.AddRange(ResolveMultiTeamTie(tiedTeamIds, resolvedMatches));
        }

        return ranking;
    }

    private static List<Guid> ResolveTwoTeamTie(List<Guid> tiedTeamIds, List<GroupStageMatch> matchesScope)
    {
        var teamA = tiedTeamIds[0];
        var teamB = tiedTeamIds[1];

        var headToHeadMatch = matchesScope.SingleOrDefault(match =>
            (match.Team1Id == teamA && match.Team2Id == teamB) ||
            (match.Team1Id == teamB && match.Team2Id == teamA));

        if (headToHeadMatch?.WinnerTeamId is Guid winnerId)
            return winnerId == teamA ? [teamA, teamB] : [teamB, teamA];

        return tiedTeamIds.OrderBy(id => id).ToList();
    }

    private static List<Guid> ResolveMultiTeamTie(List<Guid> tiedTeamIds, List<GroupStageMatch> matchesScope)
    {
        var miniTableMatches = matchesScope
            .Where(match => tiedTeamIds.Contains(match.Team1Id) && tiedTeamIds.Contains(match.Team2Id))
            .ToList();

        var miniTableStats = BuildStats(tiedTeamIds, miniTableMatches);

        var grouped = tiedTeamIds
            .GroupBy(id => new { miniTableStats[id].Wins, miniTableStats[id].PointDifference })
            .OrderByDescending(group => group.Key.Wins)
            .ThenByDescending(group => group.Key.PointDifference)
            .ToList();

        if (grouped.Count == 1)
            return tiedTeamIds.OrderBy(id => id).ToList();

        var ranking = new List<Guid>();

        foreach (var group in grouped)
        {
            var nestedTiedTeams = group.ToList();

            if (nestedTiedTeams.Count == 1)
            {
                ranking.Add(nestedTiedTeams[0]);
                continue;
            }

            if (nestedTiedTeams.Count == 2)
            {
                ranking.AddRange(ResolveTwoTeamTie(nestedTiedTeams, miniTableMatches));
                continue;
            }

            ranking.AddRange(nestedTiedTeams.OrderBy(id => id));
        }

        return ranking;
    }

    private static Dictionary<Guid, TeamStats> BuildStats(List<Guid> teamIds, List<GroupStageMatch> matches)
    {
        var stats = teamIds.ToDictionary(teamId => teamId, _ => new TeamStats());

        foreach (var match in matches)
        {
            if (!match.IsResolved)
                continue;

            if (!stats.TryGetValue(match.Team1Id, out var team1Stats) || !stats.TryGetValue(match.Team2Id, out var team2Stats))
                continue;

            var team1Score = match.Team1Score!.Value;
            var team2Score = match.Team2Score!.Value;

            team1Stats.GamesPlayed++;
            team2Stats.GamesPlayed++;

            team1Stats.PointsFor += team1Score;
            team1Stats.PointsAgainst += team2Score;

            team2Stats.PointsFor += team2Score;
            team2Stats.PointsAgainst += team1Score;

            if (team1Score > team2Score)
            {
                team1Stats.Wins++;
                team2Stats.Losses++;
            }
            else
            {
                team2Stats.Wins++;
                team1Stats.Losses++;
            }
        }

        return stats;
    }

    private sealed class TeamStats
    {
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int PointsFor { get; set; }
        public int PointsAgainst { get; set; }
        public int PointDifference => PointsFor - PointsAgainst;
    }
}
