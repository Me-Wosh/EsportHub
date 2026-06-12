using EsportHub.Domain.Tournaments;

namespace EsportHub.Domain.Matches;

public class KnockoutStageMatch : Match
{
    private KnockoutStageMatch() { }

    public KnockoutStageSide? Side { get; private set; }
    public KnockoutStageRound Round { get; private set; }

    public Guid KnockoutStageId { get; private set; }
    public KnockoutStage KnockoutStage { get; private set; } = null!;

    public static Result<KnockoutStageMatch> Create(
        Guid knockoutStageId,
        KnockoutStageRound round,
        KnockoutStageSide? side,
        Guid team1Id,
        Guid team2Id)
    {
        var knockoutStageMatch = new KnockoutStageMatch();

        return Result.Success()
            .Bind(_ => knockoutStageMatch.UpdateKnockoutStage(knockoutStageId))
            .Bind(_ => knockoutStageMatch.UpdateRound(round))
            .Bind(_ => knockoutStageMatch.UpdateSide(side, round))
            .Bind(_ => knockoutStageMatch.UpdateTeams(team1Id, team2Id));
    }

    private Result<KnockoutStageMatch> UpdateKnockoutStage(Guid knockoutStageId)
    {
        if (knockoutStageId == Guid.Empty)
            return Result.Invalid(new ValidationError("Knockout stage match must belong to a knockout stage."));

        KnockoutStageId = knockoutStageId;
        return this;
    }

    private Result<KnockoutStageMatch> UpdateRound(KnockoutStageRound round)
    {
        Round = round;
        return this;
    }

    private Result<KnockoutStageMatch> UpdateSide(KnockoutStageSide? side, KnockoutStageRound round)
    {
        if (round != KnockoutStageRound.Final && side == null)
            return Result.Invalid(new ValidationError("Non-final matches must have a side assigned."));

        if (round == KnockoutStageRound.Final && side != null)
            return Result.Invalid(new ValidationError("Final matches cannot have a side assigned."));

        Side = side;
        return this;
    }

    private Result<KnockoutStageMatch> UpdateTeams(Guid team1Id, Guid team2Id)
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
