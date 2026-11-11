namespace TodoApp.Api.Extensions;

public static class CorsExtensions
{
    public const string AngularClientPolicy = "AngularClient";

    private const string AllowedOriginsKey = "Cors:AllowedOrigins";

    /// <summary>
    /// Adds a named CORS policy that allows only the configured Angular origin(s) (NFR-14).
    /// </summary>
    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy(AngularClientPolicy, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        return services;
    }
}
