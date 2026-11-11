using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.Services;
using TodoApp.Api.Validators;

namespace TodoApp.Api.Extensions;

public static class AuthenticationExtensions
{
    private const int ClockSkewMinutes = 1;

    /// <summary>
    /// Adds ASP.NET Core Identity (password policy, lockout), JWT Bearer authentication,
    /// a fallback policy that requires an authenticated user on every endpoint (AD-4, AD-5),
    /// and the auth services.
    /// </summary>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{JwtSettings.SectionName}' is missing.");
        ValidateJwtSettings(jwtSettings);
        services.Configure<JwtSettings>(jwtSection);

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = IdentityPolicy.PasswordMinLength;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;

                options.Lockout.MaxFailedAccessAttempts = IdentityPolicy.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(IdentityPolicy.LockoutMinutes);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddPasswordValidator<LetterRequiredPasswordValidator>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep the raw JWT claim names ("sub", "email", "name").
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.FromMinutes(ClockSkewMinutes),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                };
            });

        var authenticatedUser = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(authenticatedUser)
            .SetFallbackPolicy(authenticatedUser);

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    private static void ValidateJwtSettings(JwtSettings settings)
    {
        if (Encoding.UTF8.GetByteCount(settings.Key) < JwtSettings.MinKeyBytes)
        {
            throw new InvalidOperationException(
                $"'{JwtSettings.SectionName}:Key' is missing or shorter than {JwtSettings.MinKeyBytes} bytes. Set it in user secrets or an environment variable.");
        }

        if (string.IsNullOrWhiteSpace(settings.Issuer) || string.IsNullOrWhiteSpace(settings.Audience) || settings.ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException(
                $"'{JwtSettings.SectionName}' needs Issuer, Audience and a positive ExpiryMinutes.");
        }
    }
}
