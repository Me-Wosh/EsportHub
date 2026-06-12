using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace EsportHub.Infrastructure.Twitch;

public record ChannelScheduleDto(
    IEnumerable<ChannelScheduleSegmentDto> Segments,
    string BroadcasterName,
    string BroadcasterLogin
);

public record ChannelScheduleSegmentDto(
    string Id,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Title,
    string? CategoryId,
    string? CategoryName,
    bool IsRecurring
);

public record ClipDto(string Id, string EditUrl);

public interface IStreamingSiteService
{
    Task<Result<ChannelScheduleDto>> GetRecurringSchedulesAsync(CancellationToken cancellationToken);

    Task<Result<ChannelScheduleSegmentDto>> CreateRecurringScheduleAsync(
        DateTimeOffset startTime,
        string timezone,
        int duration,
        string? title,
        string? categoryId,
        CancellationToken cancellationToken);

    Task<Result<ClipDto>> CreateClipAsync(string? title, double? duration, CancellationToken cancellationToken);
}

public class TwitchService(
    HttpClient httpClient,
    IOptions<TwitchOptions> options,
    ILogger<TwitchService> logger
) : IStreamingSiteService
{
    private readonly TwitchOptions _config = options.Value;

    public async Task<Result<ChannelScheduleDto>> GetRecurringSchedulesAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(
            $"schedule?broadcaster_id={_config.BroadcasterId}",
            cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized)
            return Result.Unauthorized();

        if (response.StatusCode is HttpStatusCode.Forbidden)
            return Result.Forbidden();
        
        if (response.StatusCode is HttpStatusCode.NotFound)
            return Result.NotFound("No recurring schedules found");

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                "Twitch API error: Status code: {StatusCode}, Response: {Response}",
                (int)response.StatusCode,
                responseContent);
            
            return Result.Error($"Twitch API error: HTTP {(int)response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<TwitchGetSchedulesResponse>(cancellationToken);

        if (data?.Data is null)
            return Result.Error("Unexpected empty response from Twitch");

        var segments = data.Data.Segments ?? [];
        var segmentDtos = segments.Select(s => new ChannelScheduleSegmentDto(
            s.Id,
            s.StartTime,
            s.EndTime,
            s.Title,
            s.Category?.Id,
            s.Category?.Name,
            s.IsRecurring
        ));

        return Result.Success(new ChannelScheduleDto(
            segmentDtos,
            data.Data.BroadcasterName,
            data.Data.BroadcasterLogin
        ));
    }

    public async Task<Result<ChannelScheduleSegmentDto>> CreateRecurringScheduleAsync(
        DateTimeOffset startTime,
        string timezone,
        int duration,
        string? title,
        string? categoryId,
        CancellationToken cancellationToken)
    {
        if (duration is < 30 or > 1380)
            return Result.Invalid(new ValidationError("Duration must be between 30 and 1380 minutes."));

        var body = new TwitchCreateSegmentRequest(startTime, timezone, duration.ToString(), true, categoryId, title);

        var response = await httpClient.PostAsJsonAsync(
            $"schedule/segment?broadcaster_id={_config.BroadcasterId}",
            body,
            cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized)
            return Result.Unauthorized();

        if (response.StatusCode is HttpStatusCode.Forbidden)
            return Result.Forbidden();

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                "Twitch API error: Status code: {StatusCode}, Response: {Response}",
                (int)response.StatusCode,
                responseContent);

            return Result.Error($"Twitch API error: HTTP {(int)response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<TwitchCreateSegmentResponse>(cancellationToken);

        // Twitch API returns list containing one, newly created segment
        var segment = data?.Data?.Segments?.SingleOrDefault();

        if (segment is null)
            return Result.Error("Unexpected empty response from Twitch");

        return Result.Success(new ChannelScheduleSegmentDto(
            segment.Id,
            segment.StartTime,
            segment.EndTime,
            segment.Title,
            segment.Category?.Id,
            segment.Category?.Name,
            segment.IsRecurring));
    }

    public async Task<Result<ClipDto>> CreateClipAsync(
        string? title,
        double? duration,
        CancellationToken cancellationToken)
    {
        var query = $"clips?broadcaster_id={_config.BroadcasterId}";

        if (!string.IsNullOrWhiteSpace(title))
            query += $"&title={Uri.EscapeDataString(title)}";

        if (duration.HasValue)
        {
            if (duration.Value is < 5 or > 60)
                return Result.Invalid(new ValidationError("Clip duration must be between 5 and 60 seconds."));

            query += $"&duration={duration.Value}";
        }

        var response = await httpClient.PostAsync(query, null, cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized)
            return Result.Unauthorized();

        if (response.StatusCode is HttpStatusCode.Forbidden)
            return Result.Forbidden();

        if (response.StatusCode is HttpStatusCode.NotFound)
            return Result.NotFound("Broadcaster is not live");

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                "Twitch API error: Status code: {StatusCode}, Response: {Response}",
                (int)response.StatusCode,
                responseContent);

            return Result.Error($"Twitch API error: {(int)response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<TwitchCreateClipResponse>(cancellationToken);

        var clip = data?.Data?.SingleOrDefault();
        if (clip is null)
            return Result.Error("Unexpected empty response from Twitch");

        return Result.Success(new ClipDto(clip.Id, clip.EditUrl));
    }

}

file record TwitchGetSchedulesResponse(
    // [property: JsonPropertyName("data")]
    TwitchScheduleData Data
);

file record TwitchScheduleData(
    // [property: JsonPropertyName("segments")]
    List<TwitchScheduleSegment>? Segments,

    [property: JsonPropertyName("broadcaster_id")]
    string BroadcasterId,
    
    [property: JsonPropertyName("broadcaster_name")]
    string BroadcasterName,
    
    [property: JsonPropertyName("broadcaster_login")]
    string BroadcasterLogin,

    Vacation? Vacation,

    Pagination Pagination
);

file record TwitchScheduleSegment(
    // [property: JsonPropertyName("id")]
    string Id,
    
    [property: JsonPropertyName("start_time")]
    DateTimeOffset StartTime,
    
    [property: JsonPropertyName("end_time")]
    DateTimeOffset EndTime,
    
    // [property: JsonPropertyName("title")]
    string Title,
    
    [property: JsonPropertyName("canceled_until")]
    string? CanceledUntil,
    
    // [property: JsonPropertyName("category")]
    TwitchScheduleCategory? Category,
    
    [property: JsonPropertyName("is_recurring")]
    bool IsRecurring
);

file record TwitchScheduleCategory(
    // [property: JsonPropertyName("id")]
    string Id,
    
    // [property: JsonPropertyName("name")]
    string Name
);

file record Vacation(
    // [property: JsonPropertyName("start_time")]
    string StartTime,
    
    // [property: JsonPropertyName("end_time")]
    string EndTime
);

file record Pagination(
    // [property: JsonPropertyName("cursor")]
    string? Cursor
);

file record TwitchCreateSegmentRequest(
    [property: JsonPropertyName("start_time")]
    DateTimeOffset StartTime,
    
    // [property: JsonPropertyName("timezone")]
    string Timezone,
    
    // [property: JsonPropertyName("duration")]
    string Duration,
    
    [property: JsonPropertyName("is_recurring")]
    bool IsRecurring,
    
    [property: JsonPropertyName("category_id")]
    string? CategoryId,
    
    // [property: JsonPropertyName("title")]
    string? Title
);

file record TwitchCreateSegmentResponse(
    // [property: JsonPropertyName("data")]
    TwitchCreateSegmentData Data
);

file record TwitchCreateSegmentData(
    // [property: JsonPropertyName("segments")]
    List<TwitchScheduleSegment>? Segments,

    [property: JsonPropertyName("broadcaster_id")]
    string BroadcasterId,
    
    [property: JsonPropertyName("broadcaster_name")]
    string BroadcasterName,
    
    [property: JsonPropertyName("broadcaster_login")]
    string BroadcasterLogin,

    Vacation? Vacation
);

file record TwitchCreateClipResponse(
    // [property: JsonPropertyName("data")]
    List<TwitchClipData> Data
);

file record TwitchClipData(
    // [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("edit_url")]
    string EditUrl
);
