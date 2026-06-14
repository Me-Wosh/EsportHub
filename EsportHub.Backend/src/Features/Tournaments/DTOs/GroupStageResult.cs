namespace EsportHub.Features.Tournaments.DTOs;

public record GroupStageResult(
    Guid Id,
    bool IsClosed,
    IEnumerable<GroupSummaryResult> Groups);

public record GroupSummaryResult(
    Guid Id,
    string Name,
    IEnumerable<TeamSummaryResult> Teams);

public record TeamSummaryResult(Guid Id, string Name);
