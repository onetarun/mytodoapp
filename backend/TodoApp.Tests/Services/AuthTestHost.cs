using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using TodoApp.Api.Data;
using TodoApp.Api.Extensions;
using TodoApp.Api.Services;
using TodoApp.Tests.Data;

namespace TodoApp.Tests.Services;

/// <summary>
/// Builds the real Identity + JWT registration (<see cref="AuthenticationExtensions.AddApiAuthentication"/>)
/// on top of an EF Core InMemory database.
/// </summary>
public sealed class AuthTestHost : IAsyncDisposable
{
    public const string Issuer = "test-issuer";

    public const string Audience = "test-audience";

    // Test-only value; the real key lives in user secrets.
    public const string SigningKey = "test-only-signing-key-0123456789-abcdefghijklmnop";

    public const int ExpiryMinutes = 60;

    private readonly ServiceProvider _provider;

    public AuthTestHost()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:Key"] = SigningKey,
                ["Jwt:ExpiryMinutes"] = ExpiryMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture),
            })
            .Build();

        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUserService>(new FakeCurrentUserService(null));
        services.AddDbContext<AppDbContext>(options => options
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        services.AddApiAuthentication(configuration);

        _provider = services.BuildServiceProvider();
    }

    public static IOptions<JwtSettings> JwtOptions => Options.Create(new JwtSettings
    {
        Issuer = Issuer,
        Audience = Audience,
        Key = SigningKey,
        ExpiryMinutes = ExpiryMinutes,
    });

    public AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    public ValueTask DisposeAsync() => _provider.DisposeAsync();
}
