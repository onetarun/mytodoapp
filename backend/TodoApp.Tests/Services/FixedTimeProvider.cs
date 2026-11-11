namespace TodoApp.Tests.Services;

/// <summary>
/// Test clock that always returns the same UTC time.
/// </summary>
public sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
}
