using EsportHub.Infrastructure.MediatR;
using EsportHub.Infrastructure.Twitch;

namespace EsportHub.Features.LiveStreams;

public record GetChannelRecurringScheduleQuery : IQuery<ChannelScheduleDto>;

public class GetChannelRecurringScheduleQueryHandler(
    IStreamingSiteService streamingSiteService
) : IQueryHandler<GetChannelRecurringScheduleQuery, ChannelScheduleDto>
{
    public async Task<Result<ChannelScheduleDto>> Handle(
        GetChannelRecurringScheduleQuery query,
        CancellationToken cancellationToken)
    {
        return await streamingSiteService.GetRecurringSchedulesAsync(cancellationToken);
    }
}
