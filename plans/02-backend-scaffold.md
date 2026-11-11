# Task 02 — Backend Solution Scaffold & Cross-Cutting Setup

**Depends on:** 01
**References:** ARCHITECTURE §4, §6 (conventions), §9 · NFR-12, NFR-14, NFR-15, NFR-17, NFR-18, NFR-27, NFR-28
**Goal:** An empty but production-shaped ASP.NET Core Web API that runs over HTTPS, with logging, error handling, CORS, rate limiting and Swagger in place.

## Steps

### Solution & projects
- [ ] Create `backend/TodoApp.sln`.
- [ ] Create `backend/TodoApp.Api` (ASP.NET Core Web API, **controllers**, not minimal APIs).
- [ ] Create `backend/TodoApp.Tests` (xUnit) and add a project reference to `TodoApp.Api`.
- [ ] Create folders in the API project: `Controllers/`, `Services/`, `Data/Entities/`, `Data/Configurations/`, `DTOs/`, `Validators/`, `Middleware/`, `Extensions/`.

### NuGet packages
- [ ] API: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation.AspNetCore` (or `FluentValidation.DependencyInjectionExtensions`), `Serilog.AspNetCore`, `Serilog.Sinks.File`, `Swashbuckle.AspNetCore`.
- [ ] Tests: `FluentAssertions`, `Microsoft.EntityFrameworkCore.InMemory` (or `Microsoft.EntityFrameworkCore.Sqlite`), `Moq`, `coverlet.collector`.

### Configuration
- [ ] `appsettings.json` sections: `ConnectionStrings:DefaultConnection`, `Jwt` (`Issuer`, `Audience`, `ExpiryMinutes: 60`), `Cors:AllowedOrigins` (`http://localhost:4200`), `RateLimiting`.
- [ ] Put the connection string and `Jwt:Key` in **user secrets** (`dotnet user-secrets init` / `set`), never in committed files.

### Cross-cutting middleware (`Program.cs` + `Extensions/`)
- [ ] **Serilog:** rolling daily file `logs/todoapp-.log`, `retainedFileCountLimit: 30`, minimum level Warning (Information in Development). (NFR-17)
- [ ] **Global exception handler** in `Middleware/`: log the full exception, return a ProblemDetails `500` with a generic message only. (NFR-18)
- [ ] Enable `AddProblemDetails()` so validation errors also return ProblemDetails.
- [ ] **HTTPS:** `UseHttpsRedirection()`, `UseHsts()` outside Development. (NFR-12)
- [ ] **CORS:** a named policy allowing only the configured Angular origin. (NFR-14)
- [ ] **Rate limiting:** built-in `AddRateLimiter` with a `login` policy (10/min per IP) and a default policy (100/min per user or IP). Return `429`. (NFR-15)
- [ ] **Swagger** in Development only, with a JWT "Authorize" (Bearer) security definition. (NFR-27)
- [ ] Set JSON to camelCase and serialize enums as strings in responses.
- [ ] Middleware order: exception handler → HTTPS → CORS → rate limiter → authentication → authorization → controllers.

### Smoke check
- [ ] Add a temporary `GET /api/v1/health` endpoint that returns `200 OK`.

## Definition of Done

- [ ] `dotnet build` succeeds with no warnings treated as errors.
- [ ] `dotnet run` serves the API over HTTPS; Swagger UI opens.
- [ ] `/api/v1/health` returns 200.
- [ ] A deliberately thrown test exception returns a ProblemDetails 500 with **no stack trace**, and the full error appears in `logs/`.
- [ ] Calling the health endpoint more than the limit returns `429`.
- [ ] No secrets in `appsettings.json` or git.
- [ ] `dotnet test` runs (0 tests is fine at this stage).
