using EsportHub.Domain.Teams;

namespace EsportHub.Domain.Tournaments;

public class GroupTeamStanding : BaseEntity
{
    private GroupTeamStanding() { }

    public int Position { get; private set; }
    public int GamesPlayed { get; private set; }
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int PointsFor { get; private set; }
    public int PointsAgainst { get; private set; }

    public Guid GroupId { get; private set; }
    public Group Group { get; private set; } = null!;

    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = null!;

    public int PointDifference => PointsFor - PointsAgainst;

    internal static Result<GroupTeamStanding> Create(Guid groupId, Guid teamId)
    {
        var groupTeamStanding = new GroupTeamStanding();

        return Result.Success()
            .Bind(_ => groupTeamStanding.UpdateGroupId(groupId))
            .Bind(_ => groupTeamStanding.UpdateTeamId(teamId));
    }

    internal Result SetStats(int gamesPlayed, int wins, int losses, int pointsFor, int pointsAgainst)
    {
        if (gamesPlayed < 0)
            return Result.Invalid(new ValidationError("Games played cannot be negative."));

        if (gamesPlayed != wins + losses)
            return Result.Invalid(new ValidationError("Games played must equal the sum of wins and losses."));

        var maxPossibleGames = GroupConstraints.TeamsRequiredCount * (GroupConstraints.TeamsRequiredCount - 1) / 2;

        if (gamesPlayed > maxPossibleGames)
        {
            return Result.Invalid(
                new ValidationError(
                    $"Games played cannot exceed the maximum possible matches in the group ({maxPossibleGames})."));
        }

        if (wins < 0)
            return Result.Invalid(new ValidationError("Wins cannot be negative."));

        if (losses < 0)
            return Result.Invalid(new ValidationError("Losses cannot be negative."));

        if (pointsFor < 0)
            return Result.Invalid(new ValidationError("Points for cannot be negative."));

        if (pointsAgainst < 0)
            return Result.Invalid(new ValidationError("Points against cannot be negative."));


        GamesPlayed = gamesPlayed;
        Wins = wins;
        Losses = losses;
        PointsFor = pointsFor;
        PointsAgainst = pointsAgainst;
        return Result.Success();
    }

    internal Result UpdatePosition(int position)
    {
        if (position <= 0)
            return Result.Invalid(new ValidationError("Position must be a positive integer."));

        if (position > GroupConstraints.TeamsRequiredCount)
        {
            return Result.Invalid(
                new ValidationError(
                    $"Position cannot exceed the number of teams in the group ({GroupConstraints.TeamsRequiredCount})."));
        }

        Position = position;
        return Result.Success();
    }

    private Result<GroupTeamStanding> UpdateGroupId(Guid groupId)
    {
        if (groupId == Guid.Empty)
            return Result.Invalid(new ValidationError("Standing must belong to a group."));

        GroupId = groupId;
        return this;
    }

    private Result<GroupTeamStanding> UpdateTeamId(Guid teamId)
    {
        if (teamId == Guid.Empty)
            return Result.Invalid(new ValidationError("Standing must belong to a team."));

        TeamId = teamId;
        return this;
    }
}
