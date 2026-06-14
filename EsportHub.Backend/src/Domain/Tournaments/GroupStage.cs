using EsportHub.Domain.Matches;
using EsportHub.Domain.Teams;

namespace EsportHub.Domain.Tournaments;

public class GroupStage : BaseEntity
{
    private readonly List<Group> _groups = [];
    private readonly Random _random = new();

    private GroupStage() { }

    public bool IsClosed { get; private set; }

    public Guid TournamentId { get; private set; }
    public Tournament Tournament { get; private set; } = null!;

    public IReadOnlyCollection<Group> Groups => _groups;

    internal static Result<GroupStage> Create(Guid tournamentId, List<string> groupNames, List<Team> teams)
    {
        var groupStage = new GroupStage();

        return Result.Success()
            .Bind(_ => groupStage.UpdateTournamentId(tournamentId))
            .Bind(_ => groupStage.InitializeGroups(groupNames, teams));
    }

    internal Result Close()
    {
        if (IsClosed)
            return Result.Invalid(new ValidationError("Group stage is already closed."));

        if (_groups.Any(g => g.Matches.Any(match => !match.IsResolved)))
            return Result.Invalid(new ValidationError("Group stage can only be closed after all matches are resolved."));

        IsClosed = true;
        return Result.Success();
    }

    internal Result<GroupStageMatch> ResolveMatch(Guid groupId, Guid matchId, int team1Score, int team2Score)
    {
        if (IsClosed)
            return Result.Invalid(new ValidationError("Cannot resolve match after group stage is closed."));

        var group = _groups.SingleOrDefault(g => g.Id == groupId);
        if (group is null)
            return Result.NotFound("Group was not found in this stage.");

        return group.ResolveMatch(matchId, team1Score, team2Score);
    }

    internal Result<List<GroupTeamStanding>> GetQualifiedTeams()
    {
        if (_groups.Any(group => group.Matches.Any(match => !match.IsResolved)))
        {
            return Result.Invalid(
                new ValidationError("All group matches must be resolved before selecting qualified teams."));
        }

        var qualifiedTeams = _groups
            .SelectMany(group => group.GetQualifiedTeams())
            .ToList();

        var expectedQualifiedTeamsCount = TournamentConstraints.GroupsRequiredCount * GroupConstraints.QualifiedTeamsCount;
        if (qualifiedTeams.Count != expectedQualifiedTeamsCount)
        {
            return Result.Invalid(
                new ValidationError(
                    $"Expected {expectedQualifiedTeamsCount} qualified teams but found {qualifiedTeams.Count}."));
        }

        return qualifiedTeams;
    }

    private Result<GroupStage> UpdateTournamentId(Guid tournamentId)
    {
        if (tournamentId == Guid.Empty)
            return Result.Invalid(new ValidationError("Group stage must belong to a tournament."));

        TournamentId = tournamentId;
        return this;
    }

    private Result<GroupStage> InitializeGroups(List<string> groupNames, List<Team> teams)
    {
        if (groupNames.Count != TournamentConstraints.GroupsRequiredCount)
        {
            return Result.Invalid(
                new ValidationError($"Exactly {TournamentConstraints.GroupsRequiredCount} groups are required."));
        }

        groupNames = groupNames.Select(gn => gn.Trim()).ToList();

        if (groupNames.Any(gn => string.IsNullOrWhiteSpace(gn)))
            return Result.Invalid(new ValidationError("Group name cannot be empty."));

        if (groupNames.Any(gn => gn.Length > GroupConstraints.NameMaxLength))
        {
            return Result.Invalid(
                new ValidationError($"Group name cannot exceed {GroupConstraints.NameMaxLength} characters."));
        }

        var teamsArray = teams.ToArray();
        _random.Shuffle(teamsArray);

        foreach (var (index, name) in groupNames.Index())
        {
            var createGroupResult = Group.Create(name, Id);
            if (!createGroupResult.IsSuccess)
                return createGroupResult.Map();

            var group = createGroupResult.Value;

            for (var i = 0; i < GroupConstraints.TeamsRequiredCount; i++)
            {
                var addTeamResult = group.AddTeam(teamsArray[i + index * GroupConstraints.TeamsRequiredCount]);
                if (!addTeamResult.IsSuccess)
                    return addTeamResult.Map();
            }

            var initializeMatchesResult = group.InitializeMatches();
            if (!initializeMatchesResult.IsSuccess)
                return initializeMatchesResult.Map();

            _groups.Add(group);
        }

        return this;
    }
}
