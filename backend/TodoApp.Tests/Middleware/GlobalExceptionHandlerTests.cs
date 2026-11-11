using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using TodoApp.Api.Middleware;

namespace TodoApp.Tests.Middleware;

public sealed class GlobalExceptionHandlerTests
{
    private const string SecretInternalMessage = "Internal failure: connection string Server=secret-db";

    private readonly Mock<ILogger<GlobalExceptionHandler>> _logger = new();

    [Fact]
    public async Task TryHandleAsync_ReturnsGenericProblemDetails500_WithoutInternalDetails()
    {
        var context = CreateHttpContext();
        var handler = new GlobalExceptionHandler(_logger.Object);

        var handled = await handler.TryHandleAsync(context, CreateThrownException(), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().StartWith("application/problem+json");

        var body = await ReadBodyAsync(context);
        body.Should().NotContain(SecretInternalMessage);
        body.Should().NotContain(nameof(CreateThrownException));
        body.Should().NotContain("   at ");

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status500InternalServerError);
        json.RootElement.GetProperty("title").GetString().Should().Be(GlobalExceptionHandler.ProblemTitle);
        json.RootElement.GetProperty("detail").GetString().Should().Be(GlobalExceptionHandler.ProblemDetail);
    }

    [Fact]
    public async Task TryHandleAsync_LogsFullExceptionAsError()
    {
        var context = CreateHttpContext();
        var handler = new GlobalExceptionHandler(_logger.Object);
        var exception = CreateThrownException();

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        _logger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static InvalidOperationException CreateThrownException()
    {
        try
        {
            throw new InvalidOperationException(SecretInternalMessage);
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
