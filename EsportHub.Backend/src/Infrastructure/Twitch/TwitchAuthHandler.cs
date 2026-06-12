using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;

namespace EsportHub.Infrastructure.Twitch;

public class TwitchAuthHandler(IMemoryCache cache) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(TwitchOptions.AccessTokenCacheKey, out string? token) && token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
