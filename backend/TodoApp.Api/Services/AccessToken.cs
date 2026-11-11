namespace TodoApp.Api.Services;

/// <summary>
/// A signed JWT and its expiry time in UTC.
/// </summary>
public sealed record AccessToken(string Token, DateTime ExpiresAt);
