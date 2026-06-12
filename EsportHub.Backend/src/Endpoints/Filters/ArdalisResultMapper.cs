namespace EsportHub.Endpoints.Filters;

public class ArdalisResultMapper : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is Ardalis.Result.IResult ardalisResult)
        {
            return ardalisResult.Status switch
            {
                ResultStatus.Ok => Results.Json(ardalisResult, statusCode: StatusCodes.Status200OK),
                ResultStatus.Created => Results.Json(ardalisResult, statusCode: StatusCodes.Status201Created),
                ResultStatus.Error => Results.Json(ardalisResult, statusCode: StatusCodes.Status400BadRequest),
                ResultStatus.Forbidden => Results.Json(ardalisResult, statusCode: StatusCodes.Status403Forbidden),
                ResultStatus.Unauthorized => Results.Json(ardalisResult, statusCode: StatusCodes.Status401Unauthorized),
                ResultStatus.Invalid => Results.Json(ardalisResult, statusCode: StatusCodes.Status400BadRequest),
                ResultStatus.NotFound => Results.Json(ardalisResult, statusCode: StatusCodes.Status404NotFound),
                ResultStatus.NoContent => Results.Json(ardalisResult, statusCode: StatusCodes.Status204NoContent),
                ResultStatus.Conflict => Results.Json(ardalisResult, statusCode: StatusCodes.Status409Conflict),
                ResultStatus.CriticalError => Results.Json(ardalisResult, statusCode: StatusCodes.Status500InternalServerError),
                ResultStatus.Unavailable => Results.Json(ardalisResult, statusCode: StatusCodes.Status503ServiceUnavailable),
                _ => Results.UnprocessableEntity(ardalisResult),
            };
        }

        return result;
    }
}
