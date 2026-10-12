using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UnitConverter.Api.Services;

/// <summary>Thrown when a request is well-formed but cannot be converted (unknown unit, etc.).</summary>
public sealed class UnitConversionException(string message) : Exception(message);

/// <summary>Maps <see cref="UnitConversionException"/> to a 400 ProblemDetails response.</summary>
public sealed class UnitConversionExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnitConversionException)
        {
            return false; // let the default handler produce a 500
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid conversion request",
                Detail = exception.Message
            }
        });
    }
}
