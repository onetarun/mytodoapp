namespace TodoApp.Api.DTOs.Auth;

/// <summary>
/// Returned by register and login. <see cref="ExpiresAt"/> is in UTC.
/// </summary>
public sealed record AuthResponse(string Token, DateTime ExpiresAt, AuthUserDto User);
