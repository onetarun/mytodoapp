# Review — Task 02: Backend Solution Scaffold & Cross-Cutting Setup

**Date:** 2026-11-11 · **Branch:** task/02-backend-scaffold · **Verdict:** ✅ PASS

## Summary
The `backend/` solution (`TodoApp.sln`, `TodoApp.Api`, `TodoApp.Tests`) targets .NET 10 LTS. It has Serilog file logging, a global exception handler returning ProblemDetails, HTTPS redirection/HSTS, a CORS policy, rate limiting, Swagger with a Bearer definition, camelCase/string-enum JSON, and a temporary health endpoint. The build, formatter and tests are clean. The required behaviours (HTTPS, Swagger, 200, 500 without a stack trace, 429, CORS) were checked against the running API. One extra package (`System.Security.Cryptography.Pkcs`) was added with client approval to fix a transitive vulnerability.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | inspect `TodoApp.Api.csproj`, `TodoApp.Tests.csproj` | ✅ both have `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 3 passed, 0 failed, 0 skipped |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none in either project |
| Coverage | — | N/A (gate starts at Task 09) |
| Frontend checks | — | N/A (`frontend/` does not exist yet) |
| Secrets search | grep `Password=`, `Pwd=`, `User Id=`, `"Key"` in `.cs`/`.json` | ✅ no matches |
| Sensitive logging | grep `logger.` calls | ✅ only `GlobalExceptionHandler` logs: method, path, trace id and the exception |
| Raw SQL | grep `FromSqlRaw`, `ExecuteSql*`, `SqlCommand`, `SqlConnection` | ✅ none |
| `[AllowAnonymous]` | grep | ✅ only `HealthController` (optional health endpoint, allowed) |
| DbContext in controllers / entities returned | grep | ✅ none (no DbContext yet) |
| One public type per file / `Async` suffix | per-file count of public types | ✅ one per file; the only async method is `TryHandleAsync` |
| TODO / FIXME / commented-out code | grep | ✅ none |
| Magic numbers | manual check | ✅ limits/window/origins/expiry come from `appsettings.json` |
| Skipped tests | grep `Skip =` | ✅ none |
| `DateTime.Now`, `RemoveRange` | grep | ✅ none |
| Branch | `git branch --show-current` | ✅ `task/02-backend-scaffold`, from `main` (48e33b2) |
| Commit messages | `git log --oneline main..HEAD` | ✅ all `[Task 02] ...` |
| Tracked build/log/secret files | `git ls-files` filter | ✅ none (`logs/` is ignored, checked with `git check-ignore`) |
| Remote / push | `git remote -v` | ✅ no remote |
| Migrations changed | — | N/A (no migrations yet) |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| `dotnet build` succeeds with no warnings | ✅ | `0 Warning(s)`, `0 Error(s)` with warnings-as-errors on |
| `dotnet run` serves the API over HTTPS; Swagger UI opens | ✅ | `https://localhost:7146` served; HTTP → `307` to HTTPS; `/swagger/index.html` → `200`; `swagger.json` has the `Bearer` scheme. Swagger UI was checked over HTTP, not visually in a browser (see Not verified) |
| `/api/v1/health` returns 200 | ✅ | `curl -sk -i https://localhost:7146/api/v1/health` → `HTTP/1.1 200 OK`; unit test `HealthControllerTests.Get_ReturnsOk` |
| Thrown exception → ProblemDetails 500 with no stack trace; full error in `logs/` | ✅ | Temporary endpoint (removed, never committed) → `500 application/problem+json` with a generic title/detail and `traceId` only. `logs/todoapp-20261111.log` has the `[ERR]` entry with the full exception and stack trace. Unit tests `TryHandleAsync_ReturnsGenericProblemDetails500_WithoutInternalDetails` and `TryHandleAsync_LogsFullExceptionAsError` cover it permanently |
| Health endpoint over the limit returns 429 | ✅ | 110 calls → 97 × `200`, 13 × `429` |
| No secrets in `appsettings.json` or git | ✅ | Connection string is empty and there is no `Jwt:Key` in `appsettings.json`; both are set in user secrets (`UserSecretsId` in csproj); grep shows no secrets |
| `dotnet test` runs | ✅ | 3 passed |

All Steps in `plans/02-backend-scaffold.md` are ticked, and each is backed by its verification table.

Step-level notes:
- Middleware order in `Program.cs`: `UseExceptionHandler` → (Swagger in Dev / `UseHsts` otherwise) → `UseHttpsRedirection` → `UseCors` → `UseRateLimiter` → `UseAuthentication` → `UseAuthorization` → `MapControllers`. ✅ Matches the required order.
- Rate limiting: `login` policy is a fixed window by IP, 10 per 60 s. The global limiter is a fixed window by user name (if authenticated) or IP, 100 per 60 s. `RejectionStatusCode = 429`. ✅ The `login` policy is attached to the login endpoint in Task 04.
- Serilog: File sink `logs/todoapp-.log`, `rollingInterval: Day`, `retainedFileCountLimit: 30`, minimum level `Warning`, and `Information` in `appsettings.Development.json`. ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Task 01 is ✅ Done in the index |
| G-PROC-2 | ✅ | `/start-process` was run by the client |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | The vulnerability blocker was raised to the client before the package was added |
| G-SCOPE-1 | ✅ | Only Task 02 items. `.editorconfig` is needed for G-CODE-2; `.gitkeep` files keep the required empty folders in git |
| G-SCOPE-2 | ✅ | No new feature |
| G-SCOPE-3 | ✅ | No Phase 2 work |
| G-SCOPE-4 | ✅ | `GUARDRAILS.md` changed only to record the approved package (required by G-DEP-2); see below |
| G-GIT-1 | ✅ | `task/02-backend-scaffold` |
| G-GIT-2 | ✅ | `[Task 02] ...` |
| G-GIT-3 | ✅ | Not merged |
| G-GIT-4 | ✅ | No remote |
| G-GIT-5 | ✅ | No `bin/`, `obj/`, `logs/` or secrets tracked |
| G-DEP-1 | ✅ | All packages are on Appendix A (Pkcs added after client approval) |
| G-DEP-2 | ✅ | `System.Security.Cryptography.Pkcs` 10.0.14 approved by the client on 2026-11-11 and recorded in Appendix A + B |
| G-DEP-3 | ✅ | All stable releases; licences MIT / Apache-2.0 / BSD-3-Clause. FluentAssertions kept at 7.2.2 (Apache-2.0) because v8 is commercial |
| G-SEC-1 | ✅ | Secrets only in user secrets |
| G-SEC-2 | ✅ | No passwords, tokens, headers or bodies logged |
| G-SEC-3 | N/A | No user data endpoints yet |
| G-SEC-4 | ✅ | No raw SQL |
| G-SEC-5 | N/A | No frontend yet |
| G-SEC-6 | ✅ | Only the optional health endpoint is anonymous. The global fallback policy is Task 04's scope |
| G-SEC-7 | ✅ | Verified: 500 response has no stack trace or exception message |
| G-SEC-8 | N/A | No resources yet |
| G-CODE-1 | ✅ | Zero warnings, nullable + warnings-as-errors |
| G-CODE-2 | ✅ | `dotnet format --verify-no-changes` passes |
| G-CODE-3 | ✅ | No business logic in controllers |
| G-CODE-4 | N/A | No entities yet |
| G-CODE-5 | ✅ | `TryHandleAsync`; PascalCase types |
| G-CODE-6 | ✅ | One public type per file, name matches |
| G-CODE-7 | ✅ | Template sample files (`WeatherForecast*`, `UnitTest1.cs`) removed; no dead code |
| G-CODE-8 | ✅ | Rate limits, window, origins and JWT expiry are in configuration |
| G-TEST-1 | ✅ | 3/3 pass, none skipped |
| G-TEST-2 | ✅ | `GlobalExceptionHandler.TryHandleAsync` has a response test and a logging test; `HealthController.Get` has a test. No service methods yet |
| G-TEST-3 | N/A | No user-data services yet |
| G-TEST-4 | N/A | From Task 09 |
| G-DB-1 to 4 | N/A | No database work yet |
| G-DESTRUCT-1 to 4 | ✅ | Only files created in this task were deleted (template samples and the temporary throw-test controller); no DB/git destructive actions |

## Issues to fix (FAIL only)
None.

## Not verified
- **Swagger UI in a browser:** `/swagger/index.html` returned `200` and `swagger.json` contains the Bearer scheme, but the page and its "Authorize" button were not opened visually. Client: run `dotnet run --project backend/TodoApp.Api --launch-profile https` and open `https://localhost:7146/swagger`.
- **HSTS outside Development:** `UseHsts()` is in the non-Development branch of `Program.cs`, but the app was only run in Development.
- **`login` rate-limit policy (10/min):** the policy is registered, but no endpoint uses it until Task 04 adds `POST /auth/login`. Its 429 behaviour is checked in Task 04.

## Files changed outside this task's scope
- `GUARDRAILS.md` — added `System.Security.Cryptography.Pkcs` to Appendix A and a Change Log row, as G-DEP-2 requires after client approval.
- `plans/00-INDEX.md` — Task 02 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
