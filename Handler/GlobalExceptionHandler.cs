using System.Net;
using System.Text.Json;
using ChatMentor.Backend.Responses;
using Microsoft.AspNetCore.Diagnostics;

namespace ChatMentor.Backend.Handler;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        // Log the full exception
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        object errorResponse;
        HttpStatusCode statusCode;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                errorResponse = JSendResponse<Dictionary<string, string[]>>.Fail(
                    validationException.Errors,
                    "One or more validation errors occurred."
                );
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                errorResponse = JSendResponse<string>.Error("You are not authorized to perform this action.");
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorResponse = JSendResponse<string>.Error("The requested resource was not found.");
                break;

            case ArgumentException or ArgumentNullException:
                statusCode = HttpStatusCode.BadRequest;
                errorResponse = JSendResponse<string>.Error("Invalid request parameters.");
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                errorResponse = JSendResponse<string>.Error("Operation is not valid in the current state.");
                break;

            case NotImplementedException:
                statusCode = HttpStatusCode.NotImplemented;
                errorResponse = JSendResponse<string>.Error("This functionality is not implemented.");
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = JSendResponse<string>.Error("An unexpected error occurred. Please try again later.");
                break;
        }
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(result, cancellationToken);

        return true;
    }
}
