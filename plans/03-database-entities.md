# Task 03 — Database: Entities, DbContext & Migrations

**Depends on:** 02
**References:** BRD §6, NFR-4, NFR-7, NFR-16, Assumption 3 · ARCHITECTURE §5, §8
**Goal:** The SQL Server database `TodoAppDb` exists with Identity tables and all app tables, soft-delete and ownership query filters, and indexes.

## Steps

### Entities (`Data/Entities/`)
- [x] `AppUser : IdentityUser` — add `Name` (required, max 100), `CreatedAt` (UTC). Identity already provides email, password hash and lockout fields.
- [x] `Priority` enum — `Low = 0`, `Medium = 1`, `High = 2`.
- [x] `Category` — `Id`, `UserId`, `Name` (required, max 50), `IsDeleted`, `DeletedAt`.
- [x] `TodoTask` — `Id`, `UserId`, `CategoryId?`, `Title` (required, max 200), `Description?` (max 2000), `DueDate?`, `Priority` (default Medium), `IsCompleted`, `CompletedAt?`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt?`, navigation `SubTasks`.
- [x] `SubTask` — `Id`, `TodoTaskId`, `Title` (required, max 200), `IsCompleted`, `SortOrder`, `IsDeleted`, `DeletedAt?`.
- [x] A shared `ISoftDeletable` interface (`IsDeleted`, `DeletedAt`) for the three app entities.

### Current user
- [x] `ICurrentUserService` / `CurrentUserService` — reads `UserId` from the JWT `sub` (`ClaimTypes.NameIdentifier`) claim via `IHttpContextAccessor`. Register it as scoped.

### DbContext (`Data/AppDbContext.cs`)
- [x] `AppDbContext : IdentityDbContext<AppUser>`, with `DbSet`s for Category, TodoTask, SubTask.
- [x] Inject `ICurrentUserService`; expose `CurrentUserId`.
- [x] **Global query filters** (AD-5, NFR-7, NFR-16):
  - Category: `c => c.UserId == CurrentUserId && !c.IsDeleted`
  - TodoTask: `t => t.UserId == CurrentUserId && !t.IsDeleted`
  - SubTask: `s => !s.IsDeleted`
- [x] Override `SaveChangesAsync`: set `CreatedAt` / `UpdatedAt` automatically, and turn `EntityState.Deleted` on `ISoftDeletable` entities into `IsDeleted = true`, `DeletedAt = UtcNow` (soft delete).

### Configurations (`Data/Configurations/`, Fluent API)
- [x] Max lengths and required fields as above.
- [x] `Priority` stored as `int`.
- [x] FKs: Category → AppUser, TodoTask → AppUser, TodoTask → Category (**nullable, `OnDelete: SetNull`/`NoAction`**), SubTask → TodoTask.
- [x] **Indexes** (NFR-4): `TodoTask(UserId, IsDeleted, IsCompleted)`, `TodoTask(UserId, DueDate)`, `Category(UserId)`, `SubTask(TodoTaskId)`.
- [x] Unique email is handled by Identity (`RequireUniqueEmail = true`, set in Task 04).

### Migration
- [x] Register `AppDbContext` with `UseSqlServer(DefaultConnection)` in `Program.cs`.
- [x] `dotnet ef migrations add InitialCreate`.
- [x] Review the generated migration (FKs, indexes, column types).
- [x] `dotnet ef database update`.

### Tests
- [x] Unit test: a soft-deleted TodoTask is not returned by queries.
- [x] Unit test: user A cannot query user B's tasks/categories (global filter).

## Definition of Done

- [x] `TodoAppDb` exists in SQL Server with `AspNetUsers`, `Categories`, `TodoTasks`, `SubTasks` and all indexes (verify in SSMS).
- [x] All `DateTime` values are saved in UTC.
- [x] Deleting an entity through the DbContext sets `IsDeleted` instead of removing the row.
- [x] Both unit tests pass.

## Notes

- Migrations live in `Data/Migrations/` (ARCHITECTURE §4): `dotnet ef migrations add InitialCreate -o Data/Migrations`.
- `ICurrentUserService` reads `ClaimTypes.NameIdentifier` (JwtBearer's mapping of `sub`) and falls back to the raw `sub` claim. It returns `null` when the request is not authenticated, so the filters then return no rows.
- FK delete behaviour: `Category → AppUser` and `TodoTask → AppUser` are `Restrict` (users are never hard deleted in v1). `TodoTask → Category` is `ClientSetNull` (`NO_ACTION` in SQL Server, which avoids multiple cascade paths). `SubTask → TodoTask` is `Cascade`. Because of the soft-delete override, a delete never reaches SQL Server; EF only uses these rules to update loaded entities (loaded subtasks are soft deleted with their task, and loaded tasks lose their category).
- UTC: `UtcDateTimeConverter` is applied to every `DateTime` property. It converts `Local` values to UTC and marks values read back as `Utc`. `CreatedAt`, `UpdatedAt` and `DeletedAt` are set from `DateTime.UtcNow`.
- `Priority` has no database default; the entity initializer sets `Medium`. (A database default with sentinel `Low` would turn an explicit `Low` into `Medium`.)

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Migration review | read `Data/Migrations/*_InitialCreate.cs` | column types/lengths, FKs and indexes as specified |
| Database | `dotnet ef database update` | `TodoAppDb` created on `localhost\SQLEXPRESS` |
| Tables | `sqlcmd ... SELECT name FROM sys.tables` | `AspNet*` (7), `Categories`, `TodoTasks`, `SubTasks`, `__EFMigrationsHistory` |
| Indexes | `sqlcmd ... sys.indexes` | `IX_TodoTasks_UserId_IsDeleted_IsCompleted`, `IX_TodoTasks_UserId_DueDate`, `IX_Categories_UserId`, `IX_SubTasks_TodoTaskId` (+ `IX_TodoTasks_CategoryId` for the FK) |
| FKs | `sqlcmd ... sys.foreign_keys` | Categories/TodoTasks → AspNetUsers `NO_ACTION`; TodoTasks → Categories `NO_ACTION`; SubTasks → TodoTasks `CASCADE` |
| Model in sync | `dotnet ef migrations has-pending-model-changes` | no changes |
| Build / format | `dotnet build --no-incremental`, `dotnet format --verify-no-changes` | 0 warnings, 0 errors; format clean |
| Tests | `dotnet test` | 18 passed (11 `AppDbContextTests`, 4 `CurrentUserServiceTests`, 3 from Task 02) |
