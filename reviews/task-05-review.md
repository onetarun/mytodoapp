# Review — Task 05: Category API

**Date:** 2026-11-11 · **Branch:** task/05-category-api · **Verdict:** ✅ PASS

## Summary
This task adds `ICategoryService`/`CategoryService` and `CategoriesController` (`GET`/`POST /api/v1/categories`, `PUT`/`DELETE /api/v1/categories/{id}`), with DTOs and FluentValidation rules. Names are required, at most 50 characters after trimming, and unique per user (case-insensitive) among non-deleted categories. Delete soft-deletes the category and moves its tasks to Uncategorized in one `SaveChangesAsync` (FR-16). Ownership comes from the existing query filters, so another user's category id gives `404`. There are 27 new tests (76 in total), and the endpoints were exercised against the real API and SQL Server.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | csproj inspection (unchanged since Task 02) | ✅ |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 76 passed, 0 failed, 0 skipped |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none |
| Secrets / logging | grep changed files | ✅ no matches; no logging added |
| `UserId` from request | grep `FromBody`/`FromQuery`/`FromRoute`; `UserId` in DTOs | ✅ none. `UserId` is set from `ICurrentUserService` in `CategoryService.CreateAsync` |
| Raw SQL | grep `FromSql*`, `ExecuteSql*`, `SqlCommand`, `SqlConnection` | ✅ none |
| `[AllowAnonymous]` | grep | ✅ only `AuthController` and `HealthController` (unchanged) |
| DbContext in controllers / entity return types | grep `Controllers/` | ✅ none; controller returns `CategoryDto` only |
| TODO / FIXME / `Skip =` / `DateTime.Now` / `RemoveRange` | grep | ✅ none |
| `.Remove(` on user data | grep | ✅ one call, `CategoryService.cs:81`. `AppDbContext` turns it into a soft delete (test `Delete_SoftDeletesCategory_...`, and SQL shows `IsDeleted = 1`) |
| Magic numbers | grep | ✅ none (comment references only); max length uses `Category.NameMaxLength` |
| Async suffix / one public type per file | grep + per-file count | ✅ |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/05-category-api` from `main` (5c666cc); 4 commits, all `[Task 05] ...` |
| Remote / tracked junk | `git remote -v`, `git ls-files` filter | ✅ no remote; nothing tracked |
| Packages | csproj diff | ✅ none changed |
| Migrations | `git diff main...HEAD --name-status` | ✅ no migration files touched |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| All four endpoints work with a JWT | ✅ (API) / ⚠️ Swagger UI click-through not done | `curl` with a Bearer token: `GET` `200`, `POST` `201` + `Location: /api/v1/categories/7`, `PUT` `200`, `DELETE` `204` (repeat → `404`). Both routes listed in `swagger.json` |
| A new user sees Work and Personal from `GET /categories` | ✅ | New user → `[{"name":"Personal",...},{"name":"Work",...}]` |
| After delete, the row still exists with `IsDeleted = 1` | ✅ | `sqlcmd`: `Categories` id 8 `Work`, `IsDeleted = 1`, `DeletedAt` set; its task kept with `CategoryId = NULL`, `IsDeleted = 0` |
| All tests in this task pass | ✅ | 76/76 |

Steps check:
- `CategoryDto(id, name, taskCount)`; `CreateCategoryRequest`/`UpdateCategoryRequest(name)` with required / max 50 / trimmed (`CategoryNameRuleExtensions`, service trims). ✅
- Unique per user, case-insensitive, among non-deleted rows (`NameExistsAsync`; tests `Create_DuplicateNameForSameUser_...`, `Create_SameNameForDifferentUser_IsAllowed`, `Create_NameOfDeletedCategory_IsAllowed`, `Update_ToAnotherExistingName_IsRejected`). ✅
- `GetAllAsync` ordered by name with non-deleted task count (`Create_ThenList_...`). ✅
- `CreateAsync` takes `UserId` from `ICurrentUserService` (`Create_SetsUserIdFromCurrentUser`). ✅
- `UpdateAsync` not-found for another user (`UserB_GetsNotFound_...`, `Update_UnknownId_ReturnsNotFound`). ✅
- `DeleteAsync` nulls `CategoryId` + soft delete in one `SaveChangesAsync` (`Delete_SoftDeletesCategory_AndMovesItsTasksToUncategorized`; real-DB check above). ✅
- Controller status codes (`CategoriesControllerTests`). ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–04 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1 | ✅ | Matches the task file and ARCHITECTURE §6 endpoints |
| G-SCOPE-2 | ✅ | No extra endpoint (no `GET /categories/{id}`); `ServiceResult<T>` is internal plumbing |
| G-SCOPE-3 | ✅ | No restore/trash (Phase 2) |
| G-SCOPE-4 | ✅ | Only `Program.cs` changed outside this task (one DI registration) |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1 | ✅ | No secrets |
| G-SEC-2 | ✅ | No logging added |
| G-SEC-3 | ✅ | `UserId` only from `ICurrentUserService`; the route `{id}` is the category id |
| G-SEC-4 | ✅ | EF LINQ only (the `sqlcmd` insert was a one-off manual verification step, not code) |
| G-SEC-5 | N/A | No frontend |
| G-SEC-6 | ✅ | Controller has no `[AllowAnonymous]`; no token → `401` |
| G-SEC-7 | ✅ | `400` ValidationProblemDetails with fixed messages; `404` ProblemDetails |
| G-SEC-8 | ✅ | Another user's id → `404` for `PUT` and `DELETE` (unit test + real API) |
| G-CODE-1, 2 | ✅ | 0 warnings; format clean |
| G-CODE-3 | ✅ | Controller calls `ICategoryService` only |
| G-CODE-4 | ✅ | DTOs only |
| G-CODE-5, 6, 7 | ✅ | Naming, one type per file, no dead code |
| G-CODE-8 | ✅ | `Category.NameMaxLength` |
| G-TEST-1 | ✅ | 76/76 |
| G-TEST-2 | ✅ | `GetAllAsync`: list + excludes other/deleted. `CreateAsync`: success + duplicate. `UpdateAsync`: success + not found + duplicate. `DeleteAsync`: success + not found |
| G-TEST-3 | ✅ | `UserB_GetsNotFound_ForUserACategory_OnUpdateAndDelete`, `GetAll_DoesNotReturnOtherUsersOrDeletedCategories` |
| G-TEST-4 | N/A | From Task 09 |
| G-DB-1, 2 | ✅ / N/A | No migrations |
| G-DB-3 | ✅ | Soft delete only |
| G-DB-4 | ✅ | `DeletedAt`/`UpdatedAt` from `DateTime.UtcNow` in `AppDbContext` |
| G-DESTRUCT-1 to 4 | ✅ | Nothing dropped or deleted. Verification data was added to `TodoAppDb` (see Not verified) |

## Issues to fix (FAIL only)
None.

## Not verified
- **Swagger UI click-through:** the endpoints are listed in Swagger, and the same calls passed with `curl` and a JWT. Client: in `/swagger`, log in, "Authorize", and try the four category endpoints.
- **Concurrent duplicate names:** uniqueness is checked in the service, not by a database index. Two simultaneous creates with the same name could both succeed. A filtered unique index would need a new migration, which this task does not specify. This is acceptable for ~100 users.
- **Verification data in `TodoAppDb`:** users `task05-a/b/c-…@example.test` with their categories, and one task (`Task 05 check task`) inserted with `sqlcmd` to test FR-16 on SQL Server.

## Files changed outside this task's scope
- `backend/TodoApp.Api/Program.cs` — registers `ICategoryService` (required).
- `plans/00-INDEX.md` — Task 05 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
