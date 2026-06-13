using System.Text.Json.Serialization;
using EsportHub.Infrastructure.Twitch;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EsportHub.Endpoints;

public class TwitchAuthEndpoints : IEndpointGroup
{
    public void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/auth/twitch");

        group.MapGet("", RedirectHttpResult (IOptions<TwitchOptions> options) =>
        {
            var config = options.Value;
            var scopes = "channel:manage:schedule clips:edit";
            var redirectUri = config.RedirectUri;
            var authUrl = $"https://id.twitch.tv/oauth2/authorize?client_id={config.ClientId}&redirect_uri={redirectUri}&response_type=code&scope={scopes}";
            return TypedResults.Redirect(authUrl);
        });

        group.MapGet("/callback", async Task<Result<string>> (
            string code,
            IOptions<TwitchOptions> options,
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken) =>
        {
            var config = options.Value;
            var httpClient = httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(
                "https://id.twitch.tv/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = config.ClientId,
                    ["client_secret"] = config.ClientSecret,
                    ["code"] = code,
                    ["grant_type"] = "authorization_code",
                    ["redirect_uri"] = config.RedirectUri
                }),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Result.Error("Failed to exchange authorization code for access token");

            var tokenData = await response.Content.ReadFromJsonAsync<TwitchTokenResponse>(cancellationToken);

            if (tokenData?.AccessToken is null)
                return Result.Error("Invalid token response from Twitch");

            cache.Set(TwitchOptions.AccessTokenCacheKey, tokenData.AccessToken);

            return Result.Success("Twitch authorization successful");
        });
    }
}

file record TwitchTokenResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken
);
