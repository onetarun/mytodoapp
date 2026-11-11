# Review — Task 03: Database: Entities, DbContext & Migrations

**Date:** 2026-11-11 · **Branch:** task/03-database-entities · **Verdict:** ✅ PASS

## Summary
This task adds the entities (`AppUser`, `Category`, `TodoTask`, `SubTask`, `Priority`, `ISoftDeletable`), `ICurrentUserService`, and `AppDbContext`. The context has ownership + soft-delete query filters, automatic UTC timestamps and soft delete on save, and a global UTC converter for `DateTime`. Fluent configurations set lengths, FKs and indexes. The `InitialCreate` migration was reviewed and applied: `TodoAppDb` now exists on `localhost\SQLEXPRESS` with all tables and indexes. 15 new tests cover soft delete, ownership isolation, timestamps, UTC storage and the current-user service.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | csproj inspection (unchanged since Task 02) | ✅ |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 18 passed, 0 failed, 0 skipped |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none |
| Model vs migration | `dotnet ef migrations has-pending-model-changes` | ✅ "No changes have been made to the model since the last migration." |
| Database objects | `sqlcmd` queries on `sys.tables`, `sys.indexes`, `sys.foreign_keys` | ✅ see Definition of Done |
| Secrets / raw SQL / TODO / `DateTime.Now` / `RemoveRange` / `Skip =` / `[FromBody]`-`[FromQuery]`-`[FromRoute]` | grep over `TodoApp.Api`, `TodoApp.Tests` (excluding generated migrations) | ✅ no matches |
| `.Remove(` on user data | grep | ✅ none in API code. Deletes go through the soft-delete override in `SaveChanges` |
| One public type per file | per-file count | ✅ 1 each (`Program.cs` 0) |
| Branch / commits | `git branch`, `git log --oneline main..HEAD` | ✅ `task/03-database-entities`; all commits `[Task 03] ...` |
| Tracked junk | `git ls-files` filter | ✅ none |
| Migrations | `git diff main...HEAD --name-status -- '**/Migrations/**'` | ✅ only `A` (new files) |
| Remote | `git remote -v` | ✅ none |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| `TodoAppDb` exists with `AspNetUsers`, `Categories`, `TodoTasks`, `SubTasks` and all indexes | ✅ (via `sqlcmd`) / ⚠️ SSMS not opened | Tables: 7 `AspNet*`, `Categories`, `TodoTasks`, `SubTasks`, `__EFMigrationsHistory`. Indexes: `IX_TodoTasks_UserId_IsDeleted_IsCompleted`, `IX_TodoTasks_UserId_DueDate`, `IX_Categories_UserId`, `IX_SubTasks_TodoTaskId` (+ FK index `IX_TodoTasks_CategoryId`) |
| All `DateTime` values are saved in UTC | ✅ | `UtcDateTimeConverter` on every `DateTime` (`ConfigureConventions`); `SaveChanges` uses `DateTime.UtcNow`. Tests `LocalDueDate_IsStoredAsUtc`, `NewTask_GetsUtcCreatedAtAndUpdatedAt_AndDefaultPriorityMedium`, `RemovedTask_..._SoftDeleted` (`DeletedAt.Kind == Utc`) |
| Deleting an entity through the DbContext sets `IsDeleted` | ✅ | `AppDbContext.ApplyAuditAndSoftDelete`; tests `RemovedTask_IsNotReturnedByQueries_ButRowIsKeptAsSoftDeleted`, `RemovedCategory_IsSoftDeleted`, `RemovedTask_SoftDeletesItsLoadedSubTasks` |
| Both unit tests pass (soft-deleted task hidden; user A cannot see user B's data) | ✅ | `RemovedTask_IsNotReturnedByQueries_ButRowIsKeptAsSoftDeleted`, `UserA_CannotQueryUserBTasks`, `UserA_CannotQueryUserBCategories`, `UnauthenticatedContext_SeesNoTasksOrCategories` all pass |

Steps check:
- Entities match BRD §6 and the task file (lengths 100/50/200/2000/200 as named constants; `Priority` Low=0/Medium=1/High=2, default Medium). ✅
- `ICurrentUserService` is registered as scoped and reads `sub`/`NameIdentifier` through `IHttpContextAccessor`. ✅
- Query filters: Category/TodoTask `UserId == CurrentUserId && !IsDeleted`; SubTask `!IsDeleted`. ✅
- FKs: Category→User, TodoTask→User (Restrict), TodoTask→Category (nullable, ClientSetNull = DB `NO_ACTION`, allowed by the task), SubTask→TodoTask (Cascade). ✅
- `AppDbContext` is registered with `UseSqlServer(DefaultConnection)`; the connection string comes from user secrets. ✅
- Migration reviewed before `database update` (G-DB-2). ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01, 02 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1 | ✅ | All items are in the task file / ARCHITECTURE §5, §8. `UtcDateTimeConverter` serves the UTC DoD item |
| G-SCOPE-2 | ✅ | No new fields or features. `Category.Tasks` / `User` navigations are EF plumbing only |
| G-SCOPE-3 | ✅ | No Phase 2 work (no restore/trash) |
| G-SCOPE-4 | ✅ | `Program.cs` (Task 02 file) changed only to register the DbContext/current user, as this task requires |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1 | ✅ | Connection string only in user secrets |
| G-SEC-2 | ✅ | No new logging |
| G-SEC-3 | ✅ | `UserId` comes only from claims via `ICurrentUserService` |
| G-SEC-4 | ✅ | No raw SQL |
| G-SEC-5 | N/A | No frontend |
| G-SEC-6 | ✅ | No new endpoints |
| G-SEC-7 | ✅ | Unchanged |
| G-SEC-8 | ✅ | Ownership filters make other users' rows invisible (basis for 404 in later tasks) |
| G-CODE-1, 2 | ✅ | 0 warnings; format clean |
| G-CODE-3 | ✅ | No controller changes |
| G-CODE-4 | N/A | No API returns yet |
| G-CODE-5 | ✅ | Interfaces `I*`; async test methods; PascalCase |
| G-CODE-6 | ✅ | One public type per file |
| G-CODE-7 | ✅ | No dead code; `.gitkeep` placeholders removed where real files now exist |
| G-CODE-8 | ✅ | Field lengths are named constants on the entities |
| G-TEST-1 | ✅ | 18/18 |
| G-TEST-2 | ✅ | `CurrentUserService.UserId`: happy path (×2) + unauthenticated + no context. `AppDbContext.SaveChanges*`: add/modify/delete paths |
| G-TEST-3 | ✅ | `UserA_CannotQueryUserBTasks`, `UserA_CannotQueryUserBCategories` |
| G-TEST-4 | N/A | From Task 09 |
| G-DB-1 | ✅ | New migration only |
| G-DB-2 | ✅ | Migration reviewed before apply |
| G-DB-3 | ✅ | Soft delete enforced centrally in `SaveChanges` |
| G-DB-4 | ✅ | UTC converter + `UtcNow` |
| G-DESTRUCT-1 | ✅ | `TodoAppDb` did not exist before (checked `sys.databases`); it was created, not dropped/reset |
| G-DESTRUCT-2 to 4 | ✅ | Only this task's `.gitkeep` placeholders removed (created in Task 02 as placeholders for exactly these folders) |

## Issues to fix (FAIL only)
None.

## Not verified
- **SSMS view:** the database, tables and indexes were checked with `sqlcmd`, not in the SSMS GUI. Client: open SSMS → `localhost\SQLEXPRESS` → Databases → `TodoAppDb` → Tables, and check the indexes on `TodoTasks`.
- **UTC on real SQL Server:** UTC conversion is tested with the EF InMemory provider, which runs the same value converter. No rows were written to `TodoAppDb`, to keep it clean. The first real rows are written in Task 04 (register).
- **Soft delete through a task's subtasks on the real database:** works for loaded subtasks (tested). Soft-deleting subtasks that are *not* loaded is the service's job in Task 06/07.

## Files changed outside this task's scope
- `backend/TodoApp.Api/Program.cs` — registers `IHttpContextAccessor`, `ICurrentUserService` and `AppDbContext` (required by the task's Migration step).
- `backend/TodoApp.Api/Services/.gitkeep`, `Data/Entities/.gitkeep`, `Data/Configurations/.gitkeep` — empty placeholders from Task 02, removed because the folders now contain real files.
- `plans/00-INDEX.md` — Task 03 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
