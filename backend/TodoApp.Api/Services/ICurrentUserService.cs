namespace TodoApp.Api.Services;

/// <summary>
/// Gives the id of the authenticated user, taken only from the JWT (G-SEC-3, AD-5).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The user id from the JWT <c>sub</c> claim, or <c>null</c> when the request is not authenticated.
    /// </summary>
    string? UserId { get; }
}
