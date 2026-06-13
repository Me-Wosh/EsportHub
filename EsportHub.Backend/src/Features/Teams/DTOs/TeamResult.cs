namespace EsportHub.Features.Teams.DTOs;

public record TeamResult(
    Guid Id,
    string Name,
    Guid TournamentId,
    string TournamentName,
    IEnumerable<PlayerResult> Players);
