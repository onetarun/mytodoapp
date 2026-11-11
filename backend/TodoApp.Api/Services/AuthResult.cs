using TodoApp.Api.DTOs.Auth;

namespace TodoApp.Api.Services;

/// <summary>
/// Outcome of a register or login call. <see cref="Response"/> is set only on success;
/// <see cref="Errors"/> (field name → messages) only when validation failed.
/// </summary>
public sealed class AuthResult
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    private AuthResult(AuthResultStatus status, AuthResponse? response, IReadOnlyDictionary<string, string[]> errors)
    {
        Status = status;
        Response = response;
        Errors = errors;
    }

    public AuthResultStatus Status { get; }

    public AuthResponse? Response { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public static AuthResult Success(AuthResponse response) => new(AuthResultStatus.Success, response, NoErrors);

    public static AuthResult ValidationFailed(IReadOnlyDictionary<string, string[]> errors) =>
        new(AuthResultStatus.ValidationFailed, null, errors);

    public static AuthResult InvalidCredentials() => new(AuthResultStatus.InvalidCredentials, null, NoErrors);

    public static AuthResult LockedOut() => new(AuthResultStatus.LockedOut, null, NoErrors);
}
