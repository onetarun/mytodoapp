# Task 02 — Backend Solution Scaffold & Cross-Cutting Setup

**Depends on:** 01
**References:** ARCHITECTURE §4, §6 (conventions), §9 · NFR-12, NFR-14, NFR-15, NFR-17, NFR-18, NFR-27, NFR-28
**Goal:** An empty but production-shaped ASP.NET Core Web API that runs over HTTPS, with logging, error handling, CORS, rate limiting and Swagger in place.

## Steps

### Solution & projects
- [x] Create `backend/TodoApp.sln`.
- [x] Create `backend/TodoApp.Api` (ASP.NET Core Web API, **controllers**, not minimal APIs).
- [x] Create `backend/TodoApp.Tests` (xUnit) and add a project reference to `TodoApp.Api`.
- [x] Create folders in the API project: `Controllers/`, `Services/`, `Data/Entities/`, `Data/Configurations/`, `DTOs/`, `Validators/`, `Middleware/`, `Extensions/`.

### NuGet packages
- [x] API: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation.AspNetCore` (or `FluentValidation.DependencyInjectionExtensions`), `Serilog.AspNetCore`, `Serilog.Sinks.File`, `Swashbuckle.AspNetCore`.
- [x] Tests: `FluentAssertions`, `Microsoft.EntityFrameworkCore.InMemory` (or `Microsoft.EntityFrameworkCore.Sqlite`), `Moq`, `coverlet.collector`.

### Configuration
- [x] `appsettings.json` sections: `ConnectionStrings:DefaultConnection`, `Jwt` (`Issuer`, `Audience`, `ExpiryMinutes: 60`), `Cors:AllowedOrigins` (`http://localhost:4200`), `RateLimiting`.
- [x] Put the connection string and `Jwt:Key` in **user secrets** (`dotnet user-secrets init` / `set`), never in committed files.

### Cross-cutting middleware (`Program.cs` + `Extensions/`)
- [x] **Serilog:** rolling daily file `logs/todoapp-.log`, `retainedFileCountLimit: 30`, minimum level Warning (Information in Development). (NFR-17)
- [x] **Global exception handler** in `Middleware/`: log the full exception, return a ProblemDetails `500` with a generic message only. (NFR-18)
- [x] Enable `AddProblemDetails()` so validation errors also return ProblemDetails.
- [x] **HTTPS:** `UseHttpsRedirection()`, `UseHsts()` outside Development. (NFR-12)
- [x] **CORS:** a named policy allowing only the configured Angular origin. (NFR-14)
- [x] **Rate limiting:** built-in `AddRateLimiter` with a `login` policy (10/min per IP) and a default policy (100/min per user or IP). Return `429`. (NFR-15)
- [x] **Swagger** in Development only, with a JWT "Authorize" (Bearer) security definition. (NFR-27)
- [x] Set JSON to camelCase and serialize enums as strings in responses.
- [x] Middleware order: exception handler → HTTPS → CORS → rate limiter → authentication → authorization → controllers.

### Smoke check
- [x] Add a temporary `GET /api/v1/health` endpoint that returns `200 OK`.

## Definition of Done

- [x] `dotnet build` succeeds with no warnings treated as errors.
- [x] `dotnet run` serves the API over HTTPS; Swagger UI opens.
- [x] `/api/v1/health` returns 200.
- [x] A deliberately thrown test exception returns a ProblemDetails 500 with **no stack trace**, and the full error appears in `logs/`.
- [x] Calling the health endpoint more than the limit returns `429`.
- [x] No secrets in `appsettings.json` or git.
- [x] `dotnet test` runs (0 tests is fine at this stage).

## Notes

- Target framework: `net10.0` (.NET 10 LTS). Microsoft packages are pinned to `10.0.11`; the latest `11.0.0` packages need .NET 11.
- `FluentAssertions` is pinned to `7.2.2` (Apache-2.0). Version 8 has a commercial licence, which G-DEP-3 does not allow.
- `System.Security.Cryptography.Pkcs` `10.0.14` is pinned directly to replace the vulnerable transitive `9.0.11` (client approved 2026-11-11, GUARDRAILS Appendix A + B).
- Rate limit values, CORS origins and JWT settings come from `appsettings.json` (G-CODE-8). Secrets (`ConnectionStrings:DefaultConnection`, `Jwt:Key`) are in user secrets (`UserSecretsId` in `TodoApp.Api.csproj`).
- Authentication schemes (JWT Bearer) and the fallback authorization policy are added in Task 04; `UseAuthentication()` / `UseAuthorization()` are already in the pipeline order.
- `backend/.editorconfig` comes from `dotnet new editorconfig`, with `insert_final_newline = true`, so `dotnet format --verify-no-changes` can run (G-CODE-2).

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Build | `dotnet build --no-incremental` | `0 Warning(s)`, `0 Error(s)` |
| Format | `dotnet format --verify-no-changes` | exit code 0 |
| Tests | `dotnet test` | 3 passed, 0 failed, 0 skipped |
| HTTPS run | `dotnet run --launch-profile https` | listens on `https://localhost:7146`; `http://localhost:5187` → `307` to HTTPS |
| Swagger | `GET /swagger/index.html`, `/swagger/v1/swagger.json` | `200`; document has the `Bearer` (http, bearer, JWT) security scheme |
| Health | `GET /api/v1/health` | `200 OK` |
| Exception | temporary `GET /api/v1/throw-test` (not committed) | `500`, `application/problem+json`, generic title/detail + `traceId`, no stack trace; full exception with stack trace in `logs/todoapp-20261111.log` |
| CORS | `OPTIONS` with `Origin: http://localhost:4200` / `http://evil.example` | `Access-Control-Allow-Origin` returned only for `localhost:4200` |
| Rate limit | 110 × `GET /api/v1/health` | 97 × `200`, 13 × `429` (limit 100/min, earlier calls in the same window counted) |
| Secrets | `dotnet user-secrets list`; grep of tracked files | secrets only in user secrets; `appsettings.json` has an empty connection string and no `Jwt:Key` |
