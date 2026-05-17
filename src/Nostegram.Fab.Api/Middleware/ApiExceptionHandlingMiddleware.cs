using Microsoft.AspNetCore.Mvc;
using Nostegram.Fab.Application.Exceptions;

namespace Nostegram.Fab.Api.Middleware;

public class ApiExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (exception is NotFoundException
                or AlreadyExistsException
                or RequiredFieldException
                or ConflictException)
            {
                logger.LogInformation(exception, "Handled API exception");
            }
            else
            {
                logger.LogError(exception, "Unhandled exception");
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "An unexpected error occurred.";
        var type = "https://httpstatuses.com/500";

        switch (exception)
        {
            case AlreadyExistsException:
                statusCode = StatusCodes.Status409Conflict;
                title = "Resource already exists.";
                type = "https://httpstatuses.com/409";
                break;

            case RequiredFieldException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Missing required field";
                type = "https://httpstatuses.com/400";
                break;

            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                title = "Not found.";
                type = "https://httpstatuses.com/404";
                break;

            case ConflictException:
                statusCode = StatusCodes.Status409Conflict;
                title = "Conflict prevented action.";
                type = "https://httpstatuses.com/409";
                break;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Type = type
        };

        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(problem);
    }
}