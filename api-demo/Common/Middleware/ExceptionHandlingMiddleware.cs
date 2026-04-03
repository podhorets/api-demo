using api_demo.Common.Exceptions;
using api_demo.Common.Models;

namespace api_demo.Common.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex) { await HandleAsync(context, ex); }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, response) = exception switch
        {
            AppValidationException ve => (400,
                new ApiErrorResponse("VALIDATION_ERROR", ve.Message, ve.Errors)),
            NotFoundException ne => (404,
                new ApiErrorResponse("NOT_FOUND", ne.Message)),
            ConflictException ce => (409,
                new ApiErrorResponse("CONFLICT", ce.Message)),
            UnauthorizedAccessException ue => (401,
                new ApiErrorResponse("UNAUTHORIZED", ue.Message)),
            _ => (500,
                new ApiErrorResponse("INTERNAL_ERROR", "An unexpected error occurred."))
        };

        if (status == 500)
            logger.LogError(exception, "Unhandled exception: {Msg}", exception.Message);
        else
            logger.LogWarning("Handled [{Status}]: {Msg}", status, exception.Message);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}