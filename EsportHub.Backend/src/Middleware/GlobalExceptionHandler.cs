using Microsoft.AspNetCore.Diagnostics;

namespace EsportHub.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception _,
        CancellationToken cancellationToken
    )
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsync("Something went wrong", cancellationToken);
        return true;
    }
}
