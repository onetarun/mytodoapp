# Task 04 — Authentication API

**Depends on:** 03
**References:** FR-1 to FR-5, FR-14, NFR-8 to NFR-11, NFR-15 · ARCHITECTURE §7, §8
**Goal:** Users can register and log in; protected endpoints require a valid 60-minute JWT; lockout and login rate limiting work.

## Steps

### Identity configuration
- [ ] `AddIdentityCore<AppUser>()` + `AddEntityFrameworkStores<AppDbContext>()` + `AddSignInManager()`.
- [ ] Password options: `RequiredLength = 8`, `RequireDigit = true`, `RequireUppercase = false`, `RequireLowercase = false`, `RequireNonAlphanumeric = false`.
- [ ] Custom `IPasswordValidator` (or FluentValidation rule) requiring **at least one letter**. (NFR-8)
- [ ] `User.RequireUniqueEmail = true`.
- [ ] Lockout: `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 min`, `AllowedForNewUsers = true`. (NFR-11)

### JWT
- [ ] `JwtSettings` options class bound from config (`Issuer`, `Audience`, `Key`, `ExpiryMinutes`).
- [ ] `ITokenService` / `TokenService` — create an HMAC-SHA256 JWT with claims `sub`, `email`, `name`, `jti`, expiry 60 min.
- [ ] `AddAuthentication(JwtBearer)` with validation of issuer, audience, lifetime and signing key; `ClockSkew` = 1 minute.
- [ ] **Global authorization fallback policy:** require authenticated user for all endpoints. (AD-5)

### DTOs & validators
- [ ] `RegisterRequest` (`name`, `email`, `password`) — validator: name required (max 100), valid email, password ≥ 8 with a letter and a digit.
- [ ] `LoginRequest` (`email`, `password`).
- [ ] `AuthResponse` (`token`, `expiresAt`, `user { id, name, email }`).

### Service & controller
- [ ] `IAuthService` / `AuthService`:
  - **Register:** create the user; on success create default categories **Work** and **Personal** (FR-14) in the same unit of work; return `AuthResponse`. Duplicate email → validation error.
  - **Login:** find the user by email → if locked out return a "locked" result → check password with lockout counting (`CheckPasswordSignInAsync(..., lockoutOnFailure: true)`) → on success return `AuthResponse`. Use a generic "Invalid email or password" message (do not reveal which one is wrong).
- [ ] `AuthController` (`/api/v1/auth`), `[AllowAnonymous]`:
  - `POST /register` → `201` with `AuthResponse`, or `400` ProblemDetails.
  - `POST /login` → `200`, `401` (invalid), or `423` (locked out). Apply the **`login` rate-limit policy**.
- [ ] Remove the temporary health endpoint from Task 02 (or keep it `[AllowAnonymous]`).

### Tests
- [ ] Register creates the user and the two default categories.
- [ ] Register with a duplicate email fails.
- [ ] Weak passwords (`abcdefgh`, `12345678`, `abc123`) are rejected.
- [ ] Login with correct credentials returns a token with the right claims and ~60 min expiry.
- [ ] 5 wrong passwords lock the account; the 6th attempt (even with the correct password) returns locked.

## Definition of Done

- [ ] Via Swagger: register → login → copy token → "Authorize" works.
- [ ] Calling any other endpoint without a token returns `401`.
- [ ] Password hashes (not plain text) are visible in `AspNetUsers`.
- [ ] 11 login calls in one minute from the same IP return `429`.
- [ ] All tests in this task pass.
