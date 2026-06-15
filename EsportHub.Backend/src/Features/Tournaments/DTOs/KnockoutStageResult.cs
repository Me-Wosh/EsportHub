using EsportHub.Domain.Tournaments;

namespace EsportHub.Features.Tournaments.DTOs;

public record KnockoutStageResult(
    Guid Id,
    bool IsClosed,
    IEnumerable<KnockoutMatchResult> Matches);

public record KnockoutMatchResult(
    Guid Id,
    KnockoutStageRound Round,
    KnockoutStageSide? Side,
    Guid Team1Id,
    string Team1Name,
    int? Team1Score,
    Guid Team2Id,
    string Team2Name,
    int? Team2Score,
    bool IsResolved);
