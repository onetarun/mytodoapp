using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApp.Api.Extensions;

/// <summary>
/// Shows the Bearer lock and a <c>401</c> response on every endpoint except <c>[AllowAnonymous]</c> ones
/// (register, login, health), matching the global fallback policy (AD-5).
/// </summary>
public sealed class BearerSecurityOperationFilter : IOperationFilter
{
    private const string UnauthorizedStatusCode = "401";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allowsAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
        if (allowsAnonymous)
        {
            return;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SwaggerExtensions.BearerSchemeName, context.Document)] = [],
        });

        operation.Responses ??= [];
        operation.Responses.TryAdd(UnauthorizedStatusCode, new OpenApiResponse { Description = "Missing, invalid or expired JWT." });
    }
}
