using Microsoft.OpenApi;

namespace TodoApp.Api.Extensions;

public static class SwaggerExtensions
{
    public const string BearerSchemeName = "Bearer";

    /// <summary>
    /// Adds Swagger generation with a JWT Bearer "Authorize" button (NFR-27). The Bearer lock and a
    /// <c>401</c> response are added only to endpoints that require authentication (see <see cref="BearerSecurityOperationFilter"/>).
    /// </summary>
    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo Application API", Version = "v1" });

            options.AddSecurityDefinition(BearerSchemeName, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the JWT returned by the login endpoint.",
            });

            options.OperationFilter<BearerSecurityOperationFilter>();
        });

        return services;
    }
}
