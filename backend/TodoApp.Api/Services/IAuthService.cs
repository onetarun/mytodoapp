using TodoApp.Api.DTOs.Auth;

namespace TodoApp.Api.Services;

public interface IAuthService
{
    /// <summary>
    /// Creates the user and the default categories "Work" and "Personal" in one transaction (FR-1, FR-14).
    /// A duplicate email or a password that breaks the policy gives <see cref="AuthResultStatus.ValidationFailed"/>.
    /// </summary>
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the credentials with lockout counting (FR-2, NFR-11). Never reveals whether the email or the password was wrong.
    /// </summary>
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
