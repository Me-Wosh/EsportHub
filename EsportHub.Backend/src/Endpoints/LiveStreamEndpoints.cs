using EsportHub.Features.LiveStreams;
using EsportHub.Infrastructure.Twitch;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EsportHub.Endpoints;

public class LiveStreamEndpoints : IEndpointGroup
{
    public void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/live-streams");

        group.MapGet("/recurring-schedules", async Task<Result<ChannelScheduleDto>> (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(new GetChannelRecurringScheduleQuery(), cancellationToken);
        });

        group.MapPost("/recurring-schedules", async Task<Result<ChannelScheduleSegmentDto>> (
            [FromBody] CreateChannelRecurringScheduleCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapPost("/clips", async Task<Result<ClipDto>> (
            [FromBody] CreateClipCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return await mediator.Send(command, cancellationToken);
        });
    }
}
