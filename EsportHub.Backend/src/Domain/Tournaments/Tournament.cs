using EsportHub.Domain.Teams;

namespace EsportHub.Domain.Tournaments;

public class Tournament : BaseEntity
{
    private Tournament() { }

    public string Name { get; private set; } = string.Empty;
    public DateTime? StartDate { get; private set; }
    public TournamentStatus Status { get; private set; } = TournamentStatus.InPreparation;

    public GroupStage? GroupStage { get; private set; }
    public KnockoutStage? KnockoutStage { get; private set; }

    public static Result<Tournament> Create(string name)
    {
        var tournament = new Tournament();

        return Result.Success()
            .Bind(_ => tournament.UpdateName(name));
    }

    public Result<Tournament> UpdateName(string name)
    {
        name = name.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError("Tournament name cannot be empty."));

        if (name.Length > TournamentConstraints.NameMaxLength)
        {
            errors.Add(
                new ValidationError($"Tournament name cannot exceed {TournamentConstraints.NameMaxLength} characters."));
        }

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Name = name;
        return this;
    }

    public Result Start(List<Team> teams, List<string> groupNames)
    {
        if (Status != TournamentStatus.InPreparation)
            return Result.Invalid(new ValidationError("Tournament can only be started from InPreparation status."));

        if (teams.Count != TournamentConstraints.TeamsRequiredCount)
        {
            return Result.Invalid(
                new ValidationError(
                    $"Tournament requires exactly {TournamentConstraints.TeamsRequiredCount} teams to start."));
        }

        if (teams.Any(team => !team.HasMinimumRoster))
        {
            return Result.Invalid(
                new ValidationError(
                    $"Every participating team must have at least {TeamConstraints.PlayersMinCount} players."));
        }

        var startGroupStageResult = StartGroupStage(groupNames, teams);
        if (!startGroupStageResult.IsSuccess)
            return startGroupStageResult.Map();

        StartDate = DateTime.UtcNow;
        return Result.Success();
    }

    public Result CloseGroupStage()
    {
        if (GroupStage is null)
            return Result.Invalid(new ValidationError("Group stage has not been initialized."));

        var closeResult = GroupStage.Close();
        if (!closeResult.IsSuccess)
            return closeResult.Map();

        var startKnockoutResult = StartKnockoutStage();
        if (!startKnockoutResult.IsSuccess)
            return startKnockoutResult.Map();

        return Result.Success();
    }

    private Result StartGroupStage(List<string> groupNames, List<Team> teams)
    {
        var groupStage = GroupStage.Create(Id, groupNames, teams);
        if (!groupStage.IsSuccess)
            return groupStage.Map();

        GroupStage = groupStage;
        Status = TournamentStatus.GroupStage;
        return Result.Success();
    }

    private Result StartKnockoutStage()
    {
        if (GroupStage is null)
            return Result.Invalid(new ValidationError("Group stage has not been initialized."));

        var qualifiedTeams = GroupStage.GetQualifiedTeams();
        if (!qualifiedTeams.IsSuccess)
            return qualifiedTeams.Map();

        var createKnockoutStageResult = KnockoutStage.Create(Id);
        if (!createKnockoutStageResult.IsSuccess)
            return createKnockoutStageResult.Map();

        var knockoutStage = createKnockoutStageResult.Value;

        var initializeQuarterFinalsResult = knockoutStage.InitializeQuarterFinals(qualifiedTeams);
        if (!initializeQuarterFinalsResult.IsSuccess)
            return initializeQuarterFinalsResult.Map();

        KnockoutStage = createKnockoutStageResult;
        Status = TournamentStatus.KnockoutStage;
        return Result.Success();
    }
}
