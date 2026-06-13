using EsportHub.Domain.Tournaments;

namespace EsportHub.Features.Tournaments.DTOs;

public record TournamentResult(Guid Id, string Name, DateTime? StartDate, TournamentStatus Status, string? Winner);
