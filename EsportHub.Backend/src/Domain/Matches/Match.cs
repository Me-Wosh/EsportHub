using Ardalis.Result;
using EsportHub.Domain.Teams;

namespace EsportHub.Domain.Matches;

public abstract class Match : BaseEntity
{
    protected Match() { }

    public MatchStageType StageType { get; protected set; }

    public int? Team1Score { get; protected set; }
    public int? Team2Score { get; protected set; }

    public Guid Team1Id { get; protected set; }
    public Team Team1 { get; protected set; } = null!;

    public Guid Team2Id { get; protected set; }
    public Team Team2 { get; protected set; } = null!;

    public bool IsResolved => Team1Score.HasValue && Team2Score.HasValue;
    public Guid? WinnerTeamId => IsResolved
        ? Team1Score > Team2Score ? Team1Id : Team2Id
        : null;

    public Result SetScores(int team1Score, int team2Score)
    {
        if (IsResolved)
            return Result.Invalid(new ValidationError("Match scores cannot be changed after the match has been resolved."));

        var errors = new List<ValidationError>();

        if (team1Score < 0)
            errors.Add(new ValidationError("Team 1 score cannot be negative."));

        if (team2Score < 0)
            errors.Add(new ValidationError("Team 2 score cannot be negative."));

        if (team1Score == team2Score)
            errors.Add(new ValidationError("Match cannot end in a draw."));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Team1Score = team1Score;
        Team2Score = team2Score;
        return Result.Success();
    }
}
