namespace TodoApp.Api.Extensions;

/// <summary>
/// JWT values bound from the "Jwt" configuration section (AD-4, NFR-10).
/// The signing key comes only from user secrets or environment variables (G-SEC-1).
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// HMAC-SHA256 needs a key of at least 256 bits.
    /// </summary>
    public const int MinKeyBytes = 32;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Key { get; init; } = string.Empty;

    public int ExpiryMinutes { get; init; }
}
