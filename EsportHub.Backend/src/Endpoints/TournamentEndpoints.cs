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
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new GetTournamentsQuery(), cancellationToken);
        });

        group.MapGet("/{id:guid}", async Task<Result<TournamentResult>> (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new GetTournamentQuery(id), cancellationToken);
        });

        group.MapPost("/", async Task<Result<TournamentResult>> (
            [FromBody] CreateTournamentCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapPut("/{id:guid}", async Task<Result<TournamentResult>> (
            Guid id,
            [FromBody] UpdateTournamentNameCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command with { Id = id }, cancellationToken);
        });
    }
}
