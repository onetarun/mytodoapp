namespace TodoApp.Api.Services;

/// <summary>
/// Outcome of a service call on a user's resource. <see cref="Value"/> is set only on success;
/// <see cref="Errors"/> (field name → messages) only when validation failed. Another user's
/// resource is reported as <see cref="ServiceResultStatus.NotFound"/> (AD-5).
/// </summary>
public sealed class ServiceResult<T>
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    private ServiceResult(ServiceResultStatus status, T? value, IReadOnlyDictionary<string, string[]> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public ServiceResultStatus Status { get; }

    public T? Value { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public static ServiceResult<T> Success(T value) => new(ServiceResultStatus.Success, value, NoErrors);

    public static ServiceResult<T> NotFound() => new(ServiceResultStatus.NotFound, default, NoErrors);

    public static ServiceResult<T> ValidationFailed(string field, string message) =>
        new(ServiceResultStatus.ValidationFailed, default, new Dictionary<string, string[]> { [field] = [message] });
}
