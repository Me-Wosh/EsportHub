namespace EsportHub.Features.Tournaments.DTOs;

public record GroupMatchResult(
    Guid Id,
    Guid Team1Id,
    string Team1Name,
    int? Team1Score,
    Guid Team2Id,
    string Team2Name,
    int? Team2Score,
    bool IsResolved);
