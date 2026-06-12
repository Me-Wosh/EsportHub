namespace EsportHub.Infrastructure.Twitch;

public class TwitchOptions
{
    public const string SectionName = "Twitch";
    public const string AccessTokenCacheKey = "twitch_access_token";
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string BroadcasterId { get; init; }
    public required string RedirectUri { get; init; }
}
