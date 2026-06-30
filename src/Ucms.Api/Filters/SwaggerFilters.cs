namespace Ucms.Api.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Controller.Action → operationId formatini yaratadi
/// </summary>
public class SwaggerOperationIdFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.OperationId =
            (context.MethodInfo.DeclaringType?.Name.Replace("Controller", "") ?? "")
            + "_" + context.MethodInfo.Name;
    }
}

/// <summary>
/// [Authorize] atributi bor endpointlarga Bearer security talabini qo'shadi
/// </summary>
public class BearerSecurityOperationFilter : IOperationFilter
{
    private static readonly OpenApiSecurityRequirement BearerRequirement = new()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer",
                }
            },
            Array.Empty<string>()
        }
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize =
            context.MethodInfo.DeclaringType!
                   .GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (!hasAuthorize) return;

        operation.Security ??= [];
        operation.Security.Add(BearerRequirement);
    }
}
