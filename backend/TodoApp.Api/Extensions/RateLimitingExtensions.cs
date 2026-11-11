using System.Threading.RateLimiting;

namespace TodoApp.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string LoginPolicy = "login";

    private const string UnknownClientKey = "unknown";

    /// <summary>
    /// Adds a "login" policy partitioned by client IP and a global default limiter
    /// partitioned by user (or IP for anonymous requests). Rejected requests get 429.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{RateLimitingSettings.SectionName}' is missing.");
        var window = TimeSpan.FromSeconds(settings.WindowSeconds);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(LoginPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => CreateFixedWindow(settings.LoginPermitLimit, window)));

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrIpKey(httpContext),
                    _ => CreateFixedWindow(settings.DefaultPermitLimit, window)));
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions CreateFixedWindow(int permitLimit, TimeSpan window) =>
        new()
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0,
        };

    private static string GetClientIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownClientKey;

    private static string GetUserOrIpKey(HttpContext httpContext)
    {
        var userName = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.Identity.Name
            : null;

        return userName is not null ? $"user:{userName}" : $"ip:{GetClientIp(httpContext)}";
    }
}
