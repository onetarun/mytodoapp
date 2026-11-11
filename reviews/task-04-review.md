# Review — Task 04: Authentication API

**Date:** 2026-11-11 · **Branch:** task/04-auth-api · **Verdict:** ✅ PASS

## Summary
This task adds ASP.NET Core Identity with the NFR-8 password policy (8+ characters, a letter and a digit) and NFR-11 lockout (5 attempts, 15 minutes). It adds a 60-minute HMAC-SHA256 JWT (`TokenService`), JWT Bearer validation with a 1-minute clock skew, and a global fallback policy that requires an authenticated user. `AuthService` and `AuthController` provide `POST /api/v1/auth/register` (201 + default categories Work/Personal in one transaction) and `POST /api/v1/auth/login` (200 / 401 / 423, `login` rate-limit policy). FluentValidation runs through a global `ValidationFilter`. There are 31 new tests (49 in total), and the full flow was checked against the real API and SQL Server.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | inspect `TodoApp.Api.csproj`, `TodoApp.Tests.csproj` | ✅ both set in both projects |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 49 passed, 0 failed, 0 skipped |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none in either project |
| Secrets | grep `Password=`, `Pwd=`, `User Id=`, `"Key":` in changed files + `appsettings*.json` | ✅ no matches. `Jwt:Key` is only in user secrets. `AuthTestHost.SigningKey` is a labelled test-only value |
| Sensitive logging | grep logger calls for password/token/authorization | ✅ none. Runtime `logs/` contains only EF SQL text with column names (no parameter values) |
| `UserId` from request | grep `FromBody`/`FromQuery`/`FromRoute`, `UserId` in DTOs | ✅ none |
| Raw SQL | grep `FromSql*`, `ExecuteSql*`, `SqlCommand`, `SqlConnection` | ✅ none |
| `[AllowAnonymous]` | grep | ✅ only `AuthController` (register + login) and `HealthController` (allowed) |
| DbContext in controllers | grep `Controllers/` | ✅ none |
| TODO / FIXME / `Skip =` / `DateTime.Now` / `RemoveRange` / `.Remove(` | grep changed files | ✅ none |
| Commented-out code | grep | ✅ none (the only hit is an XML doc comment) |
| Magic numbers | grep `5,15,60,24,10,100` in API code | ✅ only the named constants in `IdentityPolicy`. Expiry comes from `Jwt:ExpiryMinutes`; rate limits from `RateLimiting` config |
| Async suffix / one public type per file | grep + per-file count | ✅ all async methods end in `Async`; 1 public type per file (`Program.cs` 0) |
| Branch / commits | `git branch`, `git log main..HEAD --oneline` | ✅ `task/04-auth-api` from `main` (488ec0b); 4 commits, all `[Task 04] ...` |
| Remote / tracked junk | `git remote -v`, `git ls-files` filter | ✅ no remote; no `bin/obj/logs/node_modules/.bak` tracked |
| Packages | `git diff main...HEAD -- '*.csproj'` | ✅ no csproj changes; no packages added |
| Migrations | `git diff main...HEAD --name-status -- '**/Migrations/**'` | ✅ no changes (Identity tables already exist from Task 03) |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Via Swagger: register → login → copy token → "Authorize" works | ✅ (API) / ⚠️ UI click-through not done | `/swagger/index.html` loads with the "Authorize" button, and lists `/auth/register`, `/auth/login`, `/health`. The same flow was run with `curl`: register `201`, login `200`, Bearer token accepted on a protected route |
| Any other endpoint without a token returns `401` | ✅ | `GET /api/v1/categories` without a token → `401`; tampered token → `401`; valid token passes authentication (`404`, route arrives in Task 05). Fallback policy in `AuthenticationExtensions` |
| Password hashes visible in `AspNetUsers` | ✅ | `sqlcmd`: `PasswordHash` = 84-char Identity v3 PBKDF2 hash (`AQAAAAIAAYag...`), not plain text; `Register_CreatesUserWithHashedPassword_AndDefaultCategories` |
| 11 login calls in one minute from the same IP → `429` | ✅ | Fresh API process: calls 1–10 → `401`, call 11 → `429` |
| All tests in this task pass | ✅ | 49/49 |

Steps check:
- Identity: `AddIdentityCore` + EF stores + `AddSignInManager`; password `RequiredLength 8`, digit required, upper/lower/symbol not required; `LetterRequiredPasswordValidator`; `RequireUniqueEmail`; lockout 5 / 15 min / `AllowedForNewUsers`. ✅ (`Extensions/AuthenticationExtensions.cs`, `Extensions/IdentityPolicy.cs`)
- JWT: `JwtSettings` bound from `Jwt` (startup fails if the key is under 32 bytes); `TokenService` HS256 with `sub`, `email`, `name`, `jti`; issuer/audience/lifetime/key validation, `ClockSkew` 1 min; fallback policy. ✅
- DTOs and validators: `RegisterRequest` (name required, max 100; valid email; password ≥ 8 with a letter and a digit), `LoginRequest`, `AuthResponse { token, expiresAt, user { id, name, email } }`. ✅
- `AuthService`: register creates the user and Work/Personal in one transaction; duplicate email → validation error; login checks lockout first, then `CheckPasswordSignInAsync(..., lockoutOnFailure: true)`, with the generic "Invalid email or password." ✅
- `AuthController`: `[AllowAnonymous]`, `201`/`400` for register; `200`/`401`/`423` for login with `[EnableRateLimiting("login")]`. ✅
- Health endpoint kept as `[AllowAnonymous]` (allowed by the task). ✅
- Tests: default categories (`Register_CreatesUserWithHashedPassword_AndDefaultCategories`), duplicate email (`Register_DuplicateEmail_...`), weak passwords `abcdefgh`/`12345678`/`abc123` (`Register_WeakPassword_...` and `RegisterRequestValidatorTests.WeakPassword_Fails`), claims + ~60 min (`Login_CorrectCredentials_ReturnsTokenWithClaims_And60MinuteExpiry`), lockout (`Login_FiveWrongPasswords_LockAccount_AndSixthWithCorrectPasswordIsLocked`). ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–03 ✅ Done in the index |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1 | ✅ | Everything maps to the task file / ARCHITECTURE §6–8. `ValidationFilter` is the plumbing that applies FluentValidation (ARCHITECTURE §6) |
| G-SCOPE-2 | ✅ | No new feature, field or endpoint. `GET /health` already existed |
| G-SCOPE-3 | ✅ | No refresh token or other Phase 2 work |
| G-SCOPE-4 | ✅ | `Program.cs` and `HealthController.cs` (Task 02) changed; see below |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1 | ✅ | Key only in user secrets; no secrets in code or config |
| G-SEC-2 | ✅ | No logging of passwords, tokens or headers |
| G-SEC-3 | ✅ | No `UserId` in requests; the token `sub` is the server-generated Identity id |
| G-SEC-4 | ✅ | No raw SQL |
| G-SEC-5 | N/A | No frontend |
| G-SEC-6 | ✅ | Fallback policy requires authentication; only register/login (and health) are anonymous; `401` verified |
| G-SEC-7 | ✅ | `400`/`401`/`423` are ProblemDetails with fixed messages and Identity's user-facing descriptions only |
| G-SEC-8 | N/A | No per-user resources in this task |
| G-CODE-1, 2 | ✅ | 0 warnings; format clean |
| G-CODE-3 | ✅ | Controller only maps `AuthResult` to HTTP; logic is in `AuthService` |
| G-CODE-4 | ✅ | Returns `AuthResponse`/`AuthUserDto`, never `AppUser` |
| G-CODE-5 | ✅ | `RegisterAsync`, `LoginAsync`, `I*` interfaces, PascalCase |
| G-CODE-6 | ✅ | One public type per file |
| G-CODE-7 | ✅ | No dead code |
| G-CODE-8 | ✅ | `IdentityPolicy` constants, `JwtSettings.MinKeyBytes`, `ClockSkewMinutes`; expiry/rate limits from config |
| G-TEST-1 | ✅ | 49/49, none skipped |
| G-TEST-2 | ✅ | `RegisterAsync`: happy path + duplicate + weak password. `LoginAsync`: happy path + wrong password + unknown email + lockout. `TokenService.CreateToken`: valid token + fails with another key |
| G-TEST-3 | ✅ | Default categories are created only with the new user's id (asserted by `UserId` filter). Login returns only the authenticated user's own data. Category/task ownership filters were tested in Task 03 |
| G-TEST-4 | N/A | From Task 09 |
| G-DB-1 | ✅ | No migration changes |
| G-DB-2 | N/A | No new migration |
| G-DB-3 | ✅ | No deletes |
| G-DB-4 | ✅ | `ExpiresAt` and `CreatedAt` are UTC (tests check `Kind == Utc`) |
| G-DESTRUCT-1 | ✅ | No drop/reset. One check user (`task04-check-…@example.test`) and its two categories were inserted into `TodoAppDb` |
| G-DESTRUCT-2 to 4 | ✅ | Only the Task 02 placeholders `DTOs/.gitkeep` and `Validators/.gitkeep` were removed, because the folders now contain real files (same practice as Task 03) |

## Issues to fix (FAIL only)
None.

## Not verified
- **Swagger UI click-through:** the UI and its "Authorize" button were seen, but register → login → Authorize was not clicked through in the UI. The identical HTTP flow passed with `curl`. Client: open `/swagger`, register, log in, paste the token into "Authorize", and call `/api/v1/health`.
- **Default per-user limit (100/min):** not load-tested. The middleware order was fixed so the limiter now sees the authenticated user (see below).
- **15-minute unlock on the real database:** lockout was confirmed (`423`). Automatic unlock after 15 minutes was not waited for. It is configured by `DefaultLockoutTimeSpan`.

## Files changed outside this task's scope
- `backend/TodoApp.Api/Program.cs` — registers `AddApiAuthentication`, FluentValidation validators and the `ValidationFilter` (required). Also moves `UseAuthentication` before `UseRateLimiter`: before this change the user was never authenticated when the limiter ran, so the "100/min per user" limit (NFR-15) actually applied per IP.
- `backend/TodoApp.Api/Controllers/HealthController.cs` — doc comment updated only (the endpoint is kept anonymous, as the task allows).
- `backend/TodoApp.Api/DTOs/.gitkeep`, `Validators/.gitkeep` — empty Task 02 placeholders removed.
- `plans/00-INDEX.md` — Task 04 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
