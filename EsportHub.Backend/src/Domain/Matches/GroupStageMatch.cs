using EsportHub.Domain.Tournaments;

namespace EsportHub.Domain.Matches;

public class GroupStageMatch : Match
{
    private GroupStageMatch() { }

    public Guid GroupId { get; private set; }
    public Group Group { get; private set; } = null!;

    internal static Result<GroupStageMatch> Create(Guid groupId, Guid team1Id, Guid team2Id)
    {
        var groupStageMatch = new GroupStageMatch();

        return Result.Success()
            .Bind(_ => groupStageMatch.UpdateGroup(groupId))
            .Bind(_ => groupStageMatch.UpdateTeams(team1Id, team2Id));
    }

    private Result<GroupStageMatch> UpdateGroup(Guid groupId)
    {
        if (groupId == Guid.Empty)
            return Result.Invalid(new ValidationError("Group stage match must belong to a group."));

        GroupId = groupId;
        return this;
    }

    private Result<GroupStageMatch> UpdateTeams(Guid team1Id, Guid team2Id)
    {
        var errors = new List<ValidationError>();

        if (team1Id == Guid.Empty)
            errors.Add(new ValidationError("Team 1 is required."));

        if (team2Id == Guid.Empty)
            errors.Add(new ValidationError("Team 2 is required."));

        if (team1Id == team2Id && team1Id != Guid.Empty)
            errors.Add(new ValidationError("A team cannot play against itself."));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Team1Id = team1Id;
        Team2Id = team2Id;
        return this;
    }
}
