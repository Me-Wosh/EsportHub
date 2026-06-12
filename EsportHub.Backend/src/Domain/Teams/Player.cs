namespace EsportHub.Domain.Teams;

public class Player : BaseEntity
{
    private Player() { }

    public string Name { get; private set; } = string.Empty;

    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = null!;

    internal static Result<Player> Create(string name, Guid teamId)
    {
        var player = new Player();

        return Result.Success()
            .Bind(_ => player.UpdateName(name))
            .Bind(_ => player.UpdateTeam(teamId));
    }

    public Result<Player> UpdateName(string name)
    {
        name = name.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError("Player name cannot be empty."));

        if (name.Length > PlayerConstraints.NameMaxLength)
            errors.Add(new ValidationError($"Player name cannot exceed {PlayerConstraints.NameMaxLength} characters."));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Name = name;
        return this;
    }

    private Result<Player> UpdateTeam(Guid teamId)
    {
        if (teamId == Guid.Empty)
            return Result.Invalid(new ValidationError("Player must belong to a team."));

        TeamId = teamId;
        return this;
    }
}
