using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

using Serilog;

using TodoApp.Api.Data;
using TodoApp.Api.Extensions;
using TodoApp.Api.Middleware;
using TodoApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging (NFR-17): Serilog configured from the "Serilog" section.
builder.Services.AddSerilog((_, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

// Error handling (NFR-18).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Data access (AD-2) and the current user from the JWT (AD-5).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddApiSwagger();
}

var app = builder.Build();

// Middleware order: exception handler → HTTPS → CORS → rate limiter → authentication → authorization → controllers.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(CorsExtensions.AngularClientPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
