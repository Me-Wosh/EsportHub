using EsportHub.Infrastructure.MediatR;
using EsportHub.Infrastructure.Twitch;

namespace EsportHub.Features.LiveStreams;

public record CreateClipCommand(string? Title, double? Duration) : ICommand<ClipDto>;

public class CreateClipCommandHandler(
    IStreamingSiteService streamingSiteService
) : ICommandHandler<CreateClipCommand, ClipDto>
{
    public async Task<Result<ClipDto>> Handle(CreateClipCommand command, CancellationToken cancellationToken)
    {
        return await streamingSiteService.CreateClipAsync(command.Title, command.Duration, cancellationToken);
    }
}
