namespace Ucms.Api.Middlewares;

using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ucms.Domain.Exceptions;

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseUdsExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(config =>
            config.Run(async context =>
                await HandleExceptionAsync(context).ConfigureAwait(false)));

        return app;
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var exception = contextFeature.Error;
            var statusCode = exception.GetStatusCode();
            var problemDetails = GetProblemDetails(exception, (int)statusCode);

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails)).ConfigureAwait(false);

            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ExceptionHandler");
            logger.LogError(exception, "{Title}", problemDetails?.Title);
        }
    }

    public static ProblemDetails GetProblemDetails(Exception exception, int statusCode)
    {
        string? localizedMessage = null;

        if (exception is AppException appException && !string.IsNullOrWhiteSpace(appException.MessageFormat))
        {
            var args = appException.Args.Select(arg => arg.ToString() ?? arg.ToString()).ToArray();

            localizedMessage = string.Format(CultureInfo.InvariantCulture, appException.MessageFormat ?? exception.Message, args);
        }

        if (string.IsNullOrWhiteSpace(localizedMessage))
        {
            localizedMessage = exception.Message ?? exception.Message;
        }

        var (message, errors) = exception switch
        {
            ValidationException => (localizedMessage, ((ValidationException)exception)?.Errors?.ToDictionary()),
            _ => (localizedMessage, null),
        };

        return errors == null
            ? new ProblemDetails
            {
                Title = message,
                Status = statusCode
            }
            : new HttpValidationProblemDetails(errors)
            {
                Title = message,
                Status = statusCode,
            };
    }

    public static HttpStatusCode GetStatusCode(this Exception exception)
    {
        return exception switch
        {
            InsufficientBalanceException  => HttpStatusCode.BadRequest,
            CashAccountNotFoundException  => HttpStatusCode.NotFound,
            NotFoundException             => HttpStatusCode.NotFound,
            AccessDeniedException         => HttpStatusCode.Forbidden,
            ApplicationException          => HttpStatusCode.BadRequest,
            ValidationException           => HttpStatusCode.BadRequest,
            AlreadyExistException         => HttpStatusCode.Conflict,
            _                             => HttpStatusCode.InternalServerError
        };
    }

    public static IDictionary<string, string[]> ToDictionary(this IEnumerable<ValidationFailure> errors)
    {
        return errors
          .GroupBy(x => x.PropertyName)
          .ToDictionary(
            g => g.Key,
            g => g.Select(x => x.ErrorMessage).ToArray());
    }
}
