using System.Text.Json;
using System.Text.Json.Serialization;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using Serilog;

using TodoApp.Api.Data;
using TodoApp.Api.Extensions;
using TodoApp.Api.Middleware;
using TodoApp.Api.Services;
using TodoApp.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Logging (NFR-17): Serilog configured from the "Serilog" section.
builder.Services.AddSerilog((_, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

// Error handling (NFR-18).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Request validation (FluentValidation) runs before every action.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Data access (AD-2) and the current user from the JWT (AD-5).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddApiRateLimiting(builder.Configuration);
// Identity + JWT Bearer, and every endpoint requires an authenticated user by default (AD-4, AD-5).
builder.Services.AddApiAuthentication(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddApiSwagger();
}

var app = builder.Build();

// Middleware order: exception handler → HTTPS → CORS → authentication → rate limiter → authorization → controllers.
// Authentication runs before the rate limiter so the default limit is partitioned per user (NFR-15).
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
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
