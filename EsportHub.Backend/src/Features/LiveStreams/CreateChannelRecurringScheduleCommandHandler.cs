using EsportHub.Infrastructure.MediatR;
using EsportHub.Infrastructure.Twitch;

namespace EsportHub.Features.LiveStreams;

public record CreateChannelRecurringScheduleCommand(
    DateTimeOffset StartTime,
    string Timezone,
    int Duration,
    string? Title,
    string? CategoryId
) : ICommand<ChannelScheduleSegmentDto>;

public class CreateChannelRecurringScheduleCommandHandler(
    IStreamingSiteService streamingSiteService
) : ICommandHandler<CreateChannelRecurringScheduleCommand, ChannelScheduleSegmentDto>
{
    public async Task<Result<ChannelScheduleSegmentDto>> Handle(
        CreateChannelRecurringScheduleCommand command,
        CancellationToken cancellationToken)
    {
        return await streamingSiteService.CreateRecurringScheduleAsync(
            command.StartTime,
            command.Timezone,
            command.Duration,
            command.Title,
            command.CategoryId,
            cancellationToken);
    }
}
