using EsportHub.Features.Teams;
using EsportHub.Features.Teams.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EsportHub.Endpoints;

public class TeamEndpoints : IEndpointGroup
{
    public void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/teams");

        group.MapGet("/", async Task<Result<IEnumerable<TeamResult>>> (
            [FromQuery] Guid? tournamentId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetTeamsQuery(tournamentId), cancellationToken);
        });

        group.MapGet("/{id:guid}", async Task<Result<TeamResult>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetTeamQuery(id), cancellationToken);
        });

        group.MapPost("/", async Task<Result<TeamResult>> (
            [FromBody] CreateTeamCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapPut("/{id:guid}", async Task<Result<TeamResult>> (
            Guid id,
            [FromBody] UpdateTeamNameCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command with { Id = id }, cancellationToken);
        });

        group.MapPost("/{teamId:guid}/players", async Task<Result<PlayerResult>> (
            Guid teamId,
            [FromBody] AddPlayerCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command with { TeamId = teamId }, cancellationToken);
        });

        group.MapPut("/{teamId:guid}/players/{playerId:guid}", async Task<Result<PlayerResult>> (
            Guid teamId,
            Guid playerId,
            [FromBody] UpdatePlayerNameCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command with { TeamId = teamId, PlayerId = playerId }, cancellationToken);
        });

        group.MapDelete("/{teamId:guid}/players/{playerId:guid}", async Task<Result<Unit>> (
            Guid teamId,
            Guid playerId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new RemovePlayerCommand(teamId, playerId), cancellationToken);
        });

        group.MapPost("/seed", async Task<Result<IEnumerable<TeamResult>>> (
            [FromBody] SeedTeamsCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command, cancellationToken);
        });
    }
}
