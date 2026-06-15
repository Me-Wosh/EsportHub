using System.Net;
using EsportHub.Infrastructure.Twitch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace EsportHub.UnitTests.Infrastructure.Twitch;

public class TwitchServiceTests
{
    private static readonly TwitchOptions DefaultOptions = new()
    {
        ClientId = "test-client-id",
        ClientSecret = "test-client-secret",
        BroadcasterId = "123456",
        RedirectUri = "http://localhost"
    };

    [Fact]
    public async Task GetRecurringSchedulesAsync_GivenUnauthorizedResponse_ReturnsUnauthorized()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Unauthorized), out _);

        var result = await service.GetRecurringSchedulesAsync(CancellationToken.None);

        Assert.True(result.IsUnauthorized());
    }

    [Fact]
    public async Task GetRecurringSchedulesAsync_GivenForbiddenResponse_ReturnsForbidden()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Forbidden), out _);

        var result = await service.GetRecurringSchedulesAsync(CancellationToken.None);

        Assert.True(result.IsForbidden());
    }

    [Fact]
    public async Task GetRecurringSchedulesAsync_GivenNotFoundResponse_ReturnsNotFound()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.NotFound), out _);

        var result = await service.GetRecurringSchedulesAsync(CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task GetRecurringSchedulesAsync_GivenUnknownErrorResponse_ReturnsAndLogsError()
    {
        var errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request")
        };
        var service = CreateService(errorResponse, out var logger);

        var result = await service.GetRecurringSchedulesAsync(CancellationToken.None);

        Assert.True(result.IsError());
        Assert.True(ReceivedErrorLog(logger));
    }

    [Fact]
    public async Task CreateRecurringScheduleAsync_GivenDurationOutOfBounds_ReturnsInvalid()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK), out _);

        var tooShortResult = await service.CreateRecurringScheduleAsync(
            DateTimeOffset.UtcNow, "UTC", 29, null, null, CancellationToken.None);
        var tooLongResult = await service.CreateRecurringScheduleAsync(
            DateTimeOffset.UtcNow, "UTC", 1381, null, null, CancellationToken.None);

        Assert.True(tooShortResult.IsInvalid());
        Assert.True(tooLongResult.IsInvalid());
    }

    [Fact]
    public async Task CreateRecurringScheduleAsync_GivenUnauthorizedResponse_ReturnsUnauthorized()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Unauthorized), out _);

        var result = await service.CreateRecurringScheduleAsync(
            DateTimeOffset.UtcNow, "UTC", 60, null, null, CancellationToken.None);

        Assert.True(result.IsUnauthorized());
    }

    [Fact]
    public async Task CreateRecurringScheduleAsync_GivenForbiddenResponse_ReturnsForbidden()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Forbidden), out _);

        var result = await service.CreateRecurringScheduleAsync(
            DateTimeOffset.UtcNow, "UTC", 60, null, null, CancellationToken.None);

        Assert.True(result.IsForbidden());
    }

    [Fact]
    public async Task CreateRecurringScheduleAsync_GivenUnknownErrorResponse_ReturnsAndLogsError()
    {
        var errorResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("Service Unavailable")
        };
        var service = CreateService(errorResponse, out var logger);

        var result = await service.CreateRecurringScheduleAsync(
            DateTimeOffset.UtcNow, "UTC", 60, null, null, CancellationToken.None);

        Assert.True(result.IsError());
        Assert.True(ReceivedErrorLog(logger));
    }

    [Fact]
    public async Task CreateClipAsync_GivenDurationOutOfBounds_ReturnsInvalid()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK), out _);

        var tooShortResult = await service.CreateClipAsync(null, 4.9, CancellationToken.None);
        var tooLongResult = await service.CreateClipAsync(null, 60.1, CancellationToken.None);

        Assert.True(tooShortResult.IsInvalid());
        Assert.True(tooLongResult.IsInvalid());
    }

    [Fact]
    public async Task CreateClipAsync_GivenUnauthorizedResponse_ReturnsUnauthorized()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Unauthorized), out _);

        var result = await service.CreateClipAsync(null, null, CancellationToken.None);

        Assert.True(result.IsUnauthorized());
    }

    [Fact]
    public async Task CreateClipAsync_GivenForbiddenResponse_ReturnsForbidden()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Forbidden), out _);

        var result = await service.CreateClipAsync(null, null, CancellationToken.None);

        Assert.True(result.IsForbidden());
    }

    [Fact]
    public async Task CreateClipAsync_GivenNotFoundResponse_ReturnsNotFound()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.NotFound), out _);

        var result = await service.CreateClipAsync(null, null, CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task CreateClipAsync_GivenUnknownErrorResponse_ReturnsAndLogsError()
    {
        var errorResponse = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("Bad Gateway")
        };
        var service = CreateService(errorResponse, out var logger);

        var result = await service.CreateClipAsync(null, null, CancellationToken.None);

        Assert.True(result.IsError());
        Assert.True(ReceivedErrorLog(logger));
    }

    private static TwitchService CreateService(HttpResponseMessage response, out ILogger<TwitchService> logger)
    {
        var handler = Substitute.For<MockHttpMessageHandler>();
        handler.PublicSendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.twitch.tv/helix/") };
        logger = Substitute.For<ILogger<TwitchService>>();
        return new TwitchService(httpClient, Options.Create(DefaultOptions), logger);
    }

    private static bool ReceivedErrorLog(ILogger<TwitchService> logger) =>
        logger.ReceivedCalls().Any(c =>
            c.GetMethodInfo().Name == "Log" &&
            c.GetArguments()[0] is LogLevel.Error);
}

public abstract class MockHttpMessageHandler : HttpMessageHandler
{
    public abstract Task<HttpResponseMessage> PublicSendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken);

    protected sealed override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        PublicSendAsync(request, cancellationToken);
}
