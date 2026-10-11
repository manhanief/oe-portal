using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Resto.Api.Common;

/// <summary>
/// Catches any exception thrown while handling a request and returns a
/// standard "ProblemDetails" JSON response instead of a crash page.
/// Similar to Laravel's app/Exceptions/Handler.php.
///
/// In Development, the real error is included in the response so it can be
/// debugged from Swagger. In Production, users only see a generic message.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException     => (StatusCodes.Status404NotFound, "Not found"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Request cannot be completed"),
            _                     => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        var isServerError = status == StatusCodes.Status500InternalServerError;

        if (isServerError)
            logger.LogError(exception, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = !isServerError
                ? exception.Message
                : environment.IsDevelopment()
                    ? $"{exception.GetBaseException().GetType().Name}: {exception.GetBaseException().Message}"
                    : "Something went wrong. Please try again."   // never leak details in production
        };

        if (isServerError && environment.IsDevelopment())
            problem.Extensions["exception"] = exception.ToString();

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }
}
