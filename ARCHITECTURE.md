# Solution Architecture — Todo Application

**Version:** 1.0
**Date:** 2026-11-11
**Based on:** [BRD.md](BRD.md) v1.2
**Status:** Decisions approved by client

---

## 1. Architecture Drivers

| Driver | Source |
|---|---|
| ~100 users, up to 1,000 tasks per user, runs on a local machine | NFR-3, Assumption 2 |
| API < 500 ms, page < 2 s | NFR-1, NFR-2 |
| Angular SPA + ASP.NET Core Web API + EF Core + SQL Server | BRD §3 |
| JWT (60 min), hashing, lockout, rate limiting, per-user data isolation | NFR-8 to NFR-16 |
| Layered structure, xUnit tests ≥ 60%, Swagger | NFR-26 to NFR-28 |

## 2. Decision Summary

| # | Area | Decision | Alternatives considered |
|---|---|---|---|
| AD-1 | Application architecture | **Layered (N-Tier) Monolith** | Clean Architecture, Vertical Slice, Microservices |
| AD-2 | Database | **SQL Server** + EF Core Code-First | PostgreSQL, SQLite, MongoDB |
| AD-3 | API design | **RESTful, versioned `/api/v1`** | GraphQL, gRPC, REST + OData |
| AD-4 | Authentication | **ASP.NET Core Identity + JWT Bearer (Authorization header)** | JWT in HttpOnly cookie, cookie session, external IdP |
| AD-5 | Authorization | **Ownership-based** (`UserId` from JWT + EF global query filter) | Role-based (not needed in v1) |

## 3. High-Level Architecture

```
┌──────────────────────────┐   HTTPS + JSON    ┌───────────────────────────────────────┐        ┌──────────────┐
│  Angular SPA (browser)   │ ───────────────►  │  ASP.NET Core Web API (TodoApp.Api)   │  EF    │  SQL Server  │
│  - Components / Pages    │  Authorization:   │  Middleware: HTTPS, CORS, Rate limit, │ ─────► │  (local)     │
│  - Services (HttpClient) │  Bearer <JWT>     │  Exception handler, Auth              │  Core  │  TodoAppDb   │
│  - Auth interceptor      │ ◄───────────────  │  Controllers → Services → Data        │ ◄───── │              │
│  - Auth guard            │                   │  Serilog → rolling log files          │        └──────────────┘
└──────────────────────────┘                   └───────────────────────────────────────┘
```

## 4. AD-1: Layered (N-Tier) Monolith

**Why:** It is the simplest fit for the scale (~100 users), matches NFR-28, has a single deployable unit, and lets the team deliver fastest.

### Layers and responsibilities

| Layer | Responsibility | May call |
|---|---|---|
| **Controllers** | HTTP routing, model binding, returning status codes. No business logic. | Services |
| **Services** | Business rules (e.g. move tasks to Uncategorized on category delete, overdue calculation, dashboard stats). | Data (DbContext) |
| **Data** | EF Core `DbContext`, entities, configurations, migrations, global query filters. | SQL Server |
| **DTOs / Validators** | Request and response contracts; FluentValidation rules. | — |

Rule: a layer only calls the layer below it. Entities are never returned directly from the API; DTOs are always used.

### Solution structure

```
TodoApplication/
├─ BRD.md
├─ ARCHITECTURE.md
├─ backend/
│  ├─ TodoApp.sln
│  ├─ TodoApp.Api/
│  │  ├─ Controllers/        (AuthController, TasksController, SubTasksController,
│  │  │                       CategoriesController, DashboardController, NotificationsController)
│  │  ├─ Services/           (interfaces + implementations)
│  │  ├─ Data/
│  │  │  ├─ AppDbContext.cs
│  │  │  ├─ Entities/        (AppUser, Category, TodoTask, SubTask)
│  │  │  ├─ Configurations/  (Fluent API configs, indexes)
│  │  │  └─ Migrations/
│  │  ├─ DTOs/
│  │  ├─ Validators/
│  │  ├─ Middleware/         (global exception handler)
│  │  ├─ Extensions/         (service registration helpers)
│  │  ├─ appsettings.json
│  │  └─ Program.cs
│  └─ TodoApp.Tests/         (xUnit unit tests for Services)
└─ frontend/
   └─ todo-app/              (Angular SPA)
      └─ src/app/
         ├─ core/            (auth service, interceptor, guard, error handling)
         ├─ shared/          (reusable components, pipes)
         └─ features/        (auth, tasks, categories, dashboard, notifications)
```

**Trade-off accepted:** Services depend directly on EF Core `DbContext` (no repository abstraction). For tests, the EF Core InMemory provider or SQLite in-memory is used. If the app grows significantly, it can be refactored toward Clean Architecture.

## 5. AD-2: Database — SQL Server

**Why:** It is already in the BRD, has first-class EF Core support, and the data is clearly relational (User → Task → SubTask, Category FK).

| Topic | Decision |
|---|---|
| Edition | SQL Server Express or Developer (local) |
| Schema management | EF Core Code-First migrations |
| Identity tables | ASP.NET Core Identity tables (`AspNetUsers`, etc.) in the same database |
| Keys | `int` identity for Category, TodoTask, SubTask. Identity's default `string` (GUID) for User Id |
| Soft delete | `IsDeleted` + `DeletedAt` on Category, TodoTask, SubTask, with an EF Core global query filter |
| Indexes | `TodoTask(UserId, IsDeleted, IsCompleted)`, `TodoTask(UserId, DueDate)`, `Category(UserId)`, `SubTask(TodoTaskId)` |
| Dates | Stored in UTC (`datetime2`); converted to local time in the UI |
| Enums | `Priority` stored as `int` (Low = 0, Medium = 1, High = 2) |
| Backup | **Express:** Windows Task Scheduler + `sqlcmd BACKUP DATABASE` script. **Developer/Standard:** SQL Server Agent job. Keep 7 days. |
| Connection string | `appsettings.Development.json` / user secrets, never hard-coded |

## 6. AD-3: API Design — RESTful `/api/v1`

### Conventions
- JSON, camelCase, resource-based plural nouns.
- URL versioning: `/api/v1/...`.
- Standard status codes: `200 OK`, `201 Created` (with `Location` header), `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `404 Not Found`, `423 Locked` (account lockout), `429 Too Many Requests`, `500` (generic message only).
- Errors use the **ProblemDetails (RFC 7807)** format; validation errors list field-level messages.
- Request validation uses **FluentValidation**.
- **Swagger / OpenAPI** is enabled in Development, with a JWT "Authorize" button.

### Endpoints

| Method | Endpoint | Purpose | BRD |
|---|---|---|---|
| POST | `/api/v1/auth/register` | Register a user and create default categories | FR-1, FR-14 |
| POST | `/api/v1/auth/login` | Log in and return a JWT | FR-2 |
| GET | `/api/v1/tasks?status=all\|pending\|completed` | List the user's tasks | FR-10, FR-10a |
| GET | `/api/v1/tasks/{id}` | Task detail with subtasks | FR-10, FR-13 |
| POST | `/api/v1/tasks` | Create a task | FR-6 |
| PUT | `/api/v1/tasks/{id}` | Update a task | FR-7 |
| PATCH | `/api/v1/tasks/{id}/complete` | Mark complete/incomplete (`{ "isCompleted": true }`) | FR-9 |
| DELETE | `/api/v1/tasks/{id}` | Soft delete a task | FR-8, NFR-7 |
| POST | `/api/v1/tasks/{id}/subtasks` | Add a subtask | FR-11 |
| PUT | `/api/v1/tasks/{id}/subtasks/{subId}` | Edit a subtask | FR-11 |
| PATCH | `/api/v1/tasks/{id}/subtasks/{subId}/complete` | Mark a subtask done/undone | FR-12 |
| DELETE | `/api/v1/tasks/{id}/subtasks/{subId}` | Soft delete a subtask | FR-11 |
| GET | `/api/v1/categories` | List categories | FR-15 |
| POST | `/api/v1/categories` | Create a category | FR-15 |
| PUT | `/api/v1/categories/{id}` | Rename a category | FR-15 |
| DELETE | `/api/v1/categories/{id}` | Soft delete; its tasks become Uncategorized | FR-16 |
| GET | `/api/v1/dashboard/summary` | Total / Completed / Pending / Overdue / Due Today, plus upcoming 7 days | FR-17, FR-18 |
| GET | `/api/v1/notifications` | Overdue and due-within-24h tasks for the bell | FR-19, FR-20 |

Notification and dashboard data is **computed on request** (no background job), because reminders are in-app only.

## 7. AD-4: Authentication — ASP.NET Core Identity + JWT Bearer

| Topic | Decision | BRD |
|---|---|---|
| User store | ASP.NET Core Identity (`AppUser : IdentityUser` with `Name`, `CreatedAt`) | FR-1 |
| Password hashing | Identity `PasswordHasher` (PBKDF2) | NFR-9 |
| Password policy | `RequiredLength = 8`, at least one letter and one digit (custom validator). Uppercase and symbols not required | NFR-8 |
| Lockout | `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 min` | NFR-11 |
| Token | JWT, HMAC-SHA256 signed, **60 min** expiry, no refresh token in v1 | NFR-10 |
| Claims | `sub` (UserId), `email`, `name`, `jti` | — |
| Secret | JWT signing key in user secrets / environment variable, never committed | NFR-28 |
| Client storage | Token kept in **`sessionStorage`** (cleared when the tab closes), sent by an Angular HTTP interceptor as `Authorization: Bearer <token>` | FR-5 |
| Expiry handling | On a `401` response the interceptor clears the token and redirects to `/login` | NFR-10 |
| Logout | Client-side: clear the token and redirect to login | FR-5 |

### Login flow

```
Angular                          API                                  SQL Server
  │  POST /auth/login             │                                       │
  │ ────────────────────────────► │  FindByEmail, CheckPassword           │
  │                               │ ────────────────────────────────────► │
  │                               │  fail → AccessFailed (lock at 5)      │
  │                               │  success → reset count, issue JWT     │
  │ ◄──────────────────────────── │  200 { token, expiresAt, user }       │
  │  store in sessionStorage      │                                       │
  │  GET /tasks + Bearer token    │                                       │
  │ ────────────────────────────► │  validate JWT → UserId from `sub`     │
```

## 8. AD-5: Authorization — Ownership-Based

- **Global default policy:** every endpoint requires an authenticated user. Only `register` and `login` are `[AllowAnonymous]`.
- **Current user:** an `ICurrentUserService` reads `UserId` from the JWT `sub` claim.
- **Data isolation:** EF Core global query filters on Category and TodoTask: `e.UserId == currentUserId && !e.IsDeleted`. SubTask uses `!IsDeleted` and is always accessed through its parent task.
- **Writes:** `UserId` is always set on the server from the token, never taken from the request body.
- **Not found vs forbidden:** accessing another user's resource returns **404**, so the API does not reveal that the resource exists.
- **Roles:** none in v1 (single "User" role). Policies can be added later if an Admin role is needed.

## 9. Cross-Cutting Concerns

| Concern | Approach | BRD |
|---|---|---|
| HTTPS | `UseHttpsRedirection` + HSTS outside Development; Angular dev server proxies to the HTTPS API | NFR-12 |
| CORS | A named policy allowing only the Angular origin (e.g. `http://localhost:4200`) | NFR-14 |
| Rate limiting | Built-in `RateLimiter`: `login` policy 10/min per IP, default policy 100/min per user | NFR-15 |
| Error handling | Global exception handler returns ProblemDetails with a generic message; full details go only to the log | NFR-18 |
| Logging | Serilog, rolling daily file `logs/todoapp-.log`, kept for 30 days, level Warning and above (Information in Development) | NFR-17 |
| Validation | FluentValidation on the API; Angular reactive form validators with the same rules | NFR-4, NFR-13 |
| XSS | Angular's default template sanitization; no `innerHTML` with user content | NFR-13 |
| Time zone | API works in UTC; Angular converts using the browser's local time zone | Assumption 3 |
| Notification refresh | Angular polls `/notifications` every 5 minutes and on page load | FR-22 |

## 10. Frontend Architecture (Angular)

| Topic | Decision |
|---|---|
| Structure | `core` (singletons: auth, interceptor, guard), `shared` (reusable UI), `features` (lazy-loaded routes) |
| Components | Standalone components |
| Forms | Reactive Forms with validators that mirror the API rules |
| State | Feature services with RxJS / signals. No NgRx (not needed at this scale) |
| Routing | `/login`, `/register`, `/dashboard`, `/tasks`, `/tasks/:id`, `/categories`, protected by an auth guard |
| UI kit | Lightweight; Angular Material or plain CSS for a minimal, responsive look (360–1920 px) |
| Accessibility | Labels on all inputs, keyboard navigation, visible focus, overdue = red + icon |

## 11. Testing Strategy

| Level | Tooling | Scope |
|---|---|---|
| Backend unit tests | xUnit + FluentAssertions + EF Core InMemory / SQLite in-memory | Services: task CRUD, soft delete, category delete → Uncategorized, overdue logic, dashboard counts, ownership isolation. Target ≥ 60% coverage (NFR-26) |
| API manual testing | Swagger UI | All endpoints |
| Frontend tests | — | Phase 2 |

## 12. Risks & Mitigations

| Risk | Mitigation |
|---|---|
| JWT in `sessionStorage` can be stolen through XSS | Angular sanitization, no `innerHTML`, 60-minute expiry. Move to an HttpOnly cookie later if needed |
| SQL Server Express has no SQL Agent | Task Scheduler + `sqlcmd` backup script |
| Soft-deleted rows grow over time | Indexes include `IsDeleted`; purge policy is an open point (BRD §9.6) |
| Services coupled to EF Core | Accepted for v1; tests use an in-memory database |
| No refresh token: users are logged out after 60 minutes | Accepted per NFR-10; refresh tokens are in Phase 2 |
