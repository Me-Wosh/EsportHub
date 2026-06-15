using EsportHub.Features.Tournaments;
using EsportHub.Features.Tournaments.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EsportHub.Endpoints;

public class TournamentEndpoints : IEndpointGroup
{
    public void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/tournaments");

        group.MapGet("/", async Task<Result<IEnumerable<TournamentResult>>> (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetTournamentsQuery(), cancellationToken);
        });

        group.MapGet("/{id:guid}", async Task<Result<TournamentResult>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetTournamentQuery(id), cancellationToken);
        });

        group.MapPost("/", async Task<Result<TournamentResult>> (
            [FromBody] CreateTournamentCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapPut("/{id:guid}", async Task<Result<TournamentResult>> (
            Guid id,
            [FromBody] UpdateTournamentNameCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command with { Id = id }, cancellationToken);
        });

        group.MapPost("/{id:guid}/group-stage", async Task<Result<GroupStageResult>> (
            Guid id,
            [FromBody] StartGroupStageCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command with { TournamentId = id }, cancellationToken);
        });

        group.MapPost("/{id:guid}/group-stage/close", async Task<Result<GroupStageResult>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new CloseGroupStageCommand(id), cancellationToken);
        });

        group.MapGet("/{id:guid}/group-stage/groups", async Task<Result<IEnumerable<GroupResult>>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetGroupsQuery(id), cancellationToken);
        });

        group.MapGet("/{id:guid}/group-stage/groups/{groupId:guid}", async Task<Result<GroupResult>> (
            Guid id,
            Guid groupId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetGroupQuery(id, groupId), cancellationToken);
        });

        group.MapPut("/{id:guid}/group-stage/groups/{groupId:guid}", async Task<Result<GroupResult>> (
            Guid id,
            Guid groupId,
            [FromBody] UpdateGroupNameCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command with { TournamentId = id, GroupId = groupId }, cancellationToken);
        });

        group.MapGet(
            "/{id:guid}/group-stage/groups/{groupId:guid}/matches",
            async Task<Result<IEnumerable<GroupMatchResult>>> (
                Guid id,
                Guid groupId,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetGroupMatchesQuery(id, groupId), cancellationToken);
        });

        group.MapPost("/{id:guid}/group-stage/seed", async Task<Result<GroupStageResult>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new SeedGroupStageMatchesCommand(id), cancellationToken);
        });

        group.MapPut(
            "/{id:guid}/group-stage/groups/{groupId:guid}/matches/{matchId:guid}",
            async Task<Result<GroupMatchResult>> (
                Guid id,
                Guid groupId,
                Guid matchId,
                [FromBody] ResolveGroupStageMatchCommand command,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            return await mediator.Send(
                command with { TournamentId = id, GroupId = groupId, MatchId = matchId },
                cancellationToken);
        });

        group.MapGet("/{id:guid}/knockout-stage", async Task<Result<KnockoutStageResult>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetKnockoutStageQuery(id), cancellationToken);
        });

        group.MapPut(
            "/{id:guid}/knockout-stage/matches/{matchId:guid}",
            async Task<Result<KnockoutMatchResult>> (
                Guid id,
                Guid matchId,
                [FromBody] ResolveKnockoutStageMatchCommand command,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            return await mediator.Send(
                command with { TournamentId = id, MatchId = matchId },
                cancellationToken);
        });

        group.MapPost("/{id:guid}/knockout-stage/seed", async Task<Result<KnockoutStageResult>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new SeedKnockoutStageMatchesCommand(id), cancellationToken);
        });
    }
}
