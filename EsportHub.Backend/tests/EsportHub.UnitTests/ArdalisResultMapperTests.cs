using Ardalis.Result;
using EsportHub.Endpoints.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EsportHub.UnitTests;

public class ArdalisResultMapperTests
{
    [Fact]
    public async Task InvokeAsync_MapsArdalisResultToHttpIResultWithCorrectStatusCode()
    {
        // Arrange
        var filter = new ArdalisResultMapper();
        var context = new DefaultHttpContext();
        var invocationContext = EndpointFilterInvocationContext.Create(context);

        var success = Result.Success();
        var created = Result.Created("");
        var error = Result.Error();
        var forbidden = Result.Forbidden();
        var unauthorized = Result.Unauthorized();
        var invalid = Result.Invalid();
        var notFound = Result.NotFound();
        var noContent = Result.NoContent();
        var conflict = Result.Conflict();
        var criticalError = Result.CriticalError();
        var unavailable = Result.Unavailable();

        var successDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(success));
        var createdDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(created));
        var errorDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(error));
        var forbiddenDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(forbidden));
        var unauthorizedDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(unauthorized));
        var invalidDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(invalid));
        var notFoundDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(notFound));
        var noContentDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(noContent));
        var conflictDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(conflict));
        var criticalErrorDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(criticalError));
        var unavailableDelegate = new EndpointFilterDelegate((context) => ValueTask.FromResult<object?>(unavailable));

        // Act
        var successResult = await filter.InvokeAsync(invocationContext, successDelegate);
        var createdResult = await filter.InvokeAsync(invocationContext, createdDelegate);
        var errorResult = await filter.InvokeAsync(invocationContext, errorDelegate);
        var forbiddenResult = await filter.InvokeAsync(invocationContext, forbiddenDelegate);
        var unauthorizedResult = await filter.InvokeAsync(invocationContext, unauthorizedDelegate);
        var invalidResult = await filter.InvokeAsync(invocationContext, invalidDelegate);
        var notFoundResult = await filter.InvokeAsync(invocationContext, notFoundDelegate);
        var noContentResult = await filter.InvokeAsync(invocationContext, noContentDelegate);
        var conflictResult = await filter.InvokeAsync(invocationContext, conflictDelegate);
        var criticalErrorResult = await filter.InvokeAsync(invocationContext, criticalErrorDelegate);
        var unavailableResult = await filter.InvokeAsync(invocationContext, unavailableDelegate);

        // Assert
        var succesJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(successResult);
        Assert.Equal(StatusCodes.Status200OK, succesJsonResult.StatusCode);

        var createdJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(createdResult);
        Assert.Equal(StatusCodes.Status201Created, createdJsonResult.StatusCode);

        var errorJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(errorResult);
        Assert.Equal(StatusCodes.Status400BadRequest, errorJsonResult.StatusCode);

        var forbiddenJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(forbiddenResult);
        Assert.Equal(StatusCodes.Status403Forbidden, forbiddenJsonResult.StatusCode);

        var unauthorizedJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(unauthorizedResult);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedJsonResult.StatusCode);

        var invalidJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(invalidResult);
        Assert.Equal(StatusCodes.Status400BadRequest, invalidJsonResult.StatusCode);
        
        var notFoundJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(notFoundResult);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundJsonResult.StatusCode);
        
        var noContentJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(noContentResult);
        Assert.Equal(StatusCodes.Status204NoContent, noContentJsonResult.StatusCode);

        var conflictJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(conflictResult);
        Assert.Equal(StatusCodes.Status409Conflict, conflictJsonResult.StatusCode);

        var criticalErrorJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(criticalErrorResult);
        Assert.Equal(StatusCodes.Status500InternalServerError, criticalErrorJsonResult.StatusCode);

        var unavailableJsonResult = Assert.IsType<JsonHttpResult<Ardalis.Result.IResult>>(unavailableResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, unavailableJsonResult.StatusCode);
    }
}
