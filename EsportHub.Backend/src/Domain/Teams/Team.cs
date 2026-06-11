using Ardalis.Result;
using EsportHub.Domain.Tournaments;

namespace EsportHub.Domain.Teams;

public class Team : BaseEntity
{
    private readonly List<Player> _players = [];

    private Team() { }

    public string Name { get; private set; } = string.Empty;

    public Guid TournamentId { get; private set; }
    public Tournament Tournament { get; private set; } = null!;

    public IReadOnlyCollection<Player> Players => _players;

    public bool HasMinimumRoster => Players.Count >= TeamConstraints.PlayersMinCount;

    public static Result<Team> Create(string name, Guid tournamentId)
    {
        var team = new Team();

        return Result.Success()
            .Bind(_ => team.UpdateName(name))
            .Bind(_ => team.UpdateTournamentId(tournamentId));
    }

    public Result<Team> UpdateName(string name)
    {
        name = name.Trim();

        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError("Team name cannot be empty."));

        if (name.Length > TeamConstraints.NameMaxLength)
            errors.Add(new ValidationError($"Team name cannot exceed {TeamConstraints.NameMaxLength} characters."));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Name = name;
        return this;
    }

    public Result<Player> AddPlayer(string name)
    {
        if (_players.Count >= TeamConstraints.PlayersMaxCount)
            return Result.Invalid(new ValidationError($"A team can have at most {TeamConstraints.PlayersMaxCount} players."));

        var playerResult = Player.Create(name, Id);
        if (!playerResult.IsSuccess)
            return playerResult;

        _players.Add(playerResult);
        return playerResult;
    }

    public Result<Player> UpdatePlayerName(Guid playerId, string name)
    {
        var player = _players.SingleOrDefault(p => p.Id == playerId);

        if (player is null)
            return Result.NotFound("Player not found in this team.");

        return player.UpdateName(name);
    }

    public Result RemovePlayer(Guid playerId)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);

        if (player is null)
            return Result.NotFound("Player not found in this team.");

        _players.Remove(player);
        return Result.Success();
    }

    private Result<Team> UpdateTournamentId(Guid tournamentId)
    {
        if (tournamentId == Guid.Empty)
            return Result.Invalid(new ValidationError("Team must belong to a tournament."));

        TournamentId = tournamentId;
        return this;
    }
}
