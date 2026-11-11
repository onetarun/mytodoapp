namespace TodoApp.Api.Extensions;

/// <summary>
/// Rate limit values bound from the "RateLimiting" configuration section (NFR-15).
/// </summary>
public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public int LoginPermitLimit { get; init; }

    public int DefaultPermitLimit { get; init; }

    public int WindowSeconds { get; init; }
}
