# Task 03 — Database: Entities, DbContext & Migrations

**Depends on:** 02
**References:** BRD §6, NFR-4, NFR-7, NFR-16, Assumption 3 · ARCHITECTURE §5, §8
**Goal:** The SQL Server database `TodoAppDb` exists with Identity tables and all app tables, soft-delete and ownership query filters, and indexes.

## Steps

### Entities (`Data/Entities/`)
- [ ] `AppUser : IdentityUser` — add `Name` (required, max 100), `CreatedAt` (UTC). Identity already provides email, password hash and lockout fields.
- [ ] `Priority` enum — `Low = 0`, `Medium = 1`, `High = 2`.
- [ ] `Category` — `Id`, `UserId`, `Name` (required, max 50), `IsDeleted`, `DeletedAt`.
- [ ] `TodoTask` — `Id`, `UserId`, `CategoryId?`, `Title` (required, max 200), `Description?` (max 2000), `DueDate?`, `Priority` (default Medium), `IsCompleted`, `CompletedAt?`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt?`, navigation `SubTasks`.
- [ ] `SubTask` — `Id`, `TodoTaskId`, `Title` (required, max 200), `IsCompleted`, `SortOrder`, `IsDeleted`, `DeletedAt?`.
- [ ] A shared `ISoftDeletable` interface (`IsDeleted`, `DeletedAt`) for the three app entities.

### Current user
- [ ] `ICurrentUserService` / `CurrentUserService` — reads `UserId` from the JWT `sub` (`ClaimTypes.NameIdentifier`) claim via `IHttpContextAccessor`. Register it as scoped.

### DbContext (`Data/AppDbContext.cs`)
- [ ] `AppDbContext : IdentityDbContext<AppUser>`, with `DbSet`s for Category, TodoTask, SubTask.
- [ ] Inject `ICurrentUserService`; expose `CurrentUserId`.
- [ ] **Global query filters** (AD-5, NFR-7, NFR-16):
  - Category: `c => c.UserId == CurrentUserId && !c.IsDeleted`
  - TodoTask: `t => t.UserId == CurrentUserId && !t.IsDeleted`
  - SubTask: `s => !s.IsDeleted`
- [ ] Override `SaveChangesAsync`: set `CreatedAt` / `UpdatedAt` automatically, and turn `EntityState.Deleted` on `ISoftDeletable` entities into `IsDeleted = true`, `DeletedAt = UtcNow` (soft delete).

### Configurations (`Data/Configurations/`, Fluent API)
- [ ] Max lengths and required fields as above.
- [ ] `Priority` stored as `int`.
- [ ] FKs: Category → AppUser, TodoTask → AppUser, TodoTask → Category (**nullable, `OnDelete: SetNull`/`NoAction`**), SubTask → TodoTask.
- [ ] **Indexes** (NFR-4): `TodoTask(UserId, IsDeleted, IsCompleted)`, `TodoTask(UserId, DueDate)`, `Category(UserId)`, `SubTask(TodoTaskId)`.
- [ ] Unique email is handled by Identity (`RequireUniqueEmail = true`, set in Task 04).

### Migration
- [ ] Register `AppDbContext` with `UseSqlServer(DefaultConnection)` in `Program.cs`.
- [ ] `dotnet ef migrations add InitialCreate`.
- [ ] Review the generated migration (FKs, indexes, column types).
- [ ] `dotnet ef database update`.

### Tests
- [ ] Unit test: a soft-deleted TodoTask is not returned by queries.
- [ ] Unit test: user A cannot query user B's tasks/categories (global filter).

## Definition of Done

- [ ] `TodoAppDb` exists in SQL Server with `AspNetUsers`, `Categories`, `TodoTasks`, `SubTasks` and all indexes (verify in SSMS).
- [ ] All `DateTime` values are saved in UTC.
- [ ] Deleting an entity through the DbContext sets `IsDeleted` instead of removing the row.
- [ ] Both unit tests pass.
