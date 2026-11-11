namespace TodoApp.Api.Extensions;

/// <summary>
/// Password and lockout rules (NFR-8, NFR-11). Shared by Identity and the request validators.
/// </summary>
public static class IdentityPolicy
{
    public const int PasswordMinLength = 8;

    public const int MaxFailedAccessAttempts = 5;

    public const int LockoutMinutes = 15;

    public const int EmailMaxLength = 256;
}
