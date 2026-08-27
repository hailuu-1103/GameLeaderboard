using Microsoft.AspNetCore.Diagnostics;

namespace GameLeaderboard.API.ErrorHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService          problemDetailsService
)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext       httpContext,
        Exception         exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new()
            {
                Type =
                    "urn:game-leaderboard:errors:internal-server-error",
                Title    = "An unexpected error occurred.",
                Status   = StatusCodes.Status500InternalServerError,
                Detail   = "The server could not complete the request.",
                Instance = httpContext.Request.Path,
            },
        };

        return await problemDetailsService.TryWriteAsync(context);
    }
}
