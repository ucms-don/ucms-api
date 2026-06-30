namespace Ucms.Api.Middlewares;

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ucms.Domain.Exceptions;

public class GlobalMiddlewareErrorHander(RequestDelegate next, ILogger<GlobalMiddlewareErrorHander> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);

            if (IsCriticalException(ex))
                logger.LogError(ex, "Internal server error");

            else
                logger.LogInformation(ex, "Internal server error");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statusCode = ex.GetStatusCode();
        var problemDetails = ExceptionHandlerExtensions.GetProblemDetails(ex, (int)statusCode);

        // In Development: include inner exception for debugging
        var env = context.RequestServices.GetService<IWebHostEnvironment>();
        if (env?.IsDevelopment() == true && ex.InnerException is not null)
            problemDetails.Extensions["innerException"] = ex.InnerException.Message;

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;

        await response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }

    private static bool IsCriticalException(Exception ex)
    {
        return ex is not AlreadyExistException and not AccessDeniedException and not NotFoundException;
    }
}
