using TodoApp.Api.Services;

namespace TodoApp.Tests.Data;

/// <summary>
/// Test double whose user can be switched to simulate different authenticated users.
/// </summary>
public sealed class FakeCurrentUserService(string? userId) : ICurrentUserService
{
    public string? UserId { get; set; } = userId;
}
