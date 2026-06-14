namespace EsportHub.Features.Tournaments.DTOs;

public record GroupResult(
    Guid Id,
    string Name,
    IEnumerable<GroupTeamStandingResult> Standings);

public record GroupTeamStandingResult(
    int Position,
    Guid TeamId,
    string TeamName,
    int GamesPlayed,
    int Wins,
    int Losses,
    int PointsFor,
    int PointsAgainst);
