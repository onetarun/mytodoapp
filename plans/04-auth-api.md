# Task 04 — Authentication API

**Depends on:** 03
**References:** FR-1 to FR-5, FR-14, NFR-8 to NFR-11, NFR-15 · ARCHITECTURE §7, §8
**Goal:** Users can register and log in; protected endpoints require a valid 60-minute JWT; lockout and login rate limiting work.

## Steps

### Identity configuration
- [x] `AddIdentityCore<AppUser>()` + `AddEntityFrameworkStores<AppDbContext>()` + `AddSignInManager()`.
- [x] Password options: `RequiredLength = 8`, `RequireDigit = true`, `RequireUppercase = false`, `RequireLowercase = false`, `RequireNonAlphanumeric = false`.
- [x] Custom `IPasswordValidator` (or FluentValidation rule) requiring **at least one letter**. (NFR-8)
- [x] `User.RequireUniqueEmail = true`.
- [x] Lockout: `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 min`, `AllowedForNewUsers = true`. (NFR-11)

### JWT
- [x] `JwtSettings` options class bound from config (`Issuer`, `Audience`, `Key`, `ExpiryMinutes`).
- [x] `ITokenService` / `TokenService` — create an HMAC-SHA256 JWT with claims `sub`, `email`, `name`, `jti`, expiry 60 min.
- [x] `AddAuthentication(JwtBearer)` with validation of issuer, audience, lifetime and signing key; `ClockSkew` = 1 minute.
- [x] **Global authorization fallback policy:** require authenticated user for all endpoints. (AD-5)

### DTOs & validators
- [x] `RegisterRequest` (`name`, `email`, `password`) — validator: name required (max 100), valid email, password ≥ 8 with a letter and a digit.
- [x] `LoginRequest` (`email`, `password`).
- [x] `AuthResponse` (`token`, `expiresAt`, `user { id, name, email }`).

### Service & controller
- [x] `IAuthService` / `AuthService`:
  - **Register:** create the user; on success create default categories **Work** and **Personal** (FR-14) in the same unit of work; return `AuthResponse`. Duplicate email → validation error.
  - **Login:** find the user by email → if locked out return a "locked" result → check password with lockout counting (`CheckPasswordSignInAsync(..., lockoutOnFailure: true)`) → on success return `AuthResponse`. Use a generic "Invalid email or password" message (do not reveal which one is wrong).
- [x] `AuthController` (`/api/v1/auth`), `[AllowAnonymous]`:
  - `POST /register` → `201` with `AuthResponse`, or `400` ProblemDetails.
  - `POST /login` → `200`, `401` (invalid), or `423` (locked out). Apply the **`login` rate-limit policy**.
- [x] Remove the temporary health endpoint from Task 02 (or keep it `[AllowAnonymous]`).

### Tests
- [x] Register creates the user and the two default categories.
- [x] Register with a duplicate email fails.
- [x] Weak passwords (`abcdefgh`, `12345678`, `abc123`) are rejected.
- [x] Login with correct credentials returns a token with the right claims and ~60 min expiry.
- [x] 5 wrong passwords lock the account; the 6th attempt (even with the correct password) returns locked.

## Definition of Done

- [x] Via Swagger: register → login → copy token → "Authorize" works.
- [x] Calling any other endpoint without a token returns `401`.
- [x] Password hashes (not plain text) are visible in `AspNetUsers`.
- [x] 11 login calls in one minute from the same IP return `429`.
- [x] All tests in this task pass.

## Notes

- Identity, JWT Bearer, the fallback policy and the auth services are registered in `Extensions/AuthenticationExtensions.AddApiAuthentication`. Startup fails fast if `Jwt:Key` is missing or shorter than 32 bytes.
- Password and lockout numbers are named constants in `Extensions/IdentityPolicy` (G-CODE-8); token expiry comes from `Jwt:ExpiryMinutes`.
- The letter rule is enforced twice: `RegisterRequestValidator` (field errors for the client) and `LetterRequiredPasswordValidator` (Identity level).
- FluentValidation runs through a global `ValidationFilter` that returns `400` ValidationProblemDetails with camelCase field names.
- Register runs `CreateAsync` and the default-category insert in one database transaction.
- JwtBearer keeps raw claim names (`MapInboundClaims = false`) and uses `sub` as the name claim, so `User.Identity.Name` is the user id.
- `Program.cs` (Task 02 file): `UseAuthentication` now runs **before** `UseRateLimiter`. Before, the user was never authenticated when the limiter ran, so the default 100/min limit was always per IP instead of per user (NFR-15).
- The Task 02 health endpoint is kept and stays `[AllowAnonymous]`.

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Build / format | `dotnet build --no-incremental`, `dotnet format --verify-no-changes` | 0 warnings, 0 errors; format clean |
| Tests | `dotnet test` | 49 passed, 0 failed, 0 skipped |
| Register / login (real API + SQL Server) | `curl` against `http://localhost:5187` | register `201`; duplicate `400` with `errors.email`; invalid body `400` with `name`/`email`/`password` errors; login `200`, token claims `sub`, `email`, `name`, `jti`, `iss`, `aud`, `exp` = now + 60 min |
| Token on protected route | `GET /api/v1/categories` | no token `401`; tampered token `401`; valid token passes auth (`404` because the route comes in Task 05) |
| Lockout | 5 wrong logins, then the correct password | `401 401 401 401 423`, then `423` |
| Rate limit | 11 `POST /auth/login` in a fresh window | calls 1–10 `401`, call 11 `429` |
| Password hash | `sqlcmd` on `AspNetUsers` | `PasswordHash` is an 84-char Identity v3 (PBKDF2) hash, not plain text; `Work` and `Personal` categories created |
| Swagger | open `/swagger/index.html` | UI loads with the "Authorize" button; `/auth/register`, `/auth/login`, `/health` listed. The click-through in the UI was not done (same flow verified with `curl`) |
| Logs | grep `logs/` for passwords/tokens | only EF SQL statements with column names; no parameter values, passwords or tokens |
