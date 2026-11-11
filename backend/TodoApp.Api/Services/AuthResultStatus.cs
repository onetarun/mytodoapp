namespace TodoApp.Api.Services;

public enum AuthResultStatus
{
    Success,
    ValidationFailed,
    InvalidCredentials,
    LockedOut,
}
