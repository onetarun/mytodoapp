# Review — Task 06: Task API

**Date:** 2026-11-11 · **Branch:** task/06-task-api · **Verdict:** ✅ PASS

## Summary
This task adds `ITaskService`/`TaskService`, `TasksController` (six endpoints under `/api/v1/tasks`), the task DTOs, a status filter, and shared create/update validation that includes the category ownership check. It also adds `TaskRules`, the shared overdue rule for Task 08. Reads are projected to DTOs with `AsNoTracking()`. Ownership and soft delete come from the existing query filters, so another user's task gives `404`. There are 31 new tests (107 in total). All endpoints were checked against the real API and SQL Server, and `GET /tasks` with 1,000 tasks answers in about 11–25 ms.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | csproj inspection (unchanged) | ✅ |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 107 passed, 0 failed, 0 skipped |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none |
| Secrets / logging | grep changed files | ✅ no matches; no logging added |
| `UserId` from request | grep `FromBody`/`FromQuery`/`FromRoute`; `UserId` in DTOs | ✅ none. `UserId` is set from `ICurrentUserService` in `TaskService.CreateAsync` |
| Raw SQL | grep | ✅ none |
| `[AllowAnonymous]` | grep | ✅ only `AuthController`, `HealthController` |
| DbContext in controllers / entity return types | grep | ✅ none; DTOs only |
| TODO / FIXME / `Skip =` / `DateTime.Now` / `RemoveRange` | grep | ✅ none |
| `.Remove(` on user data | grep | ✅ `TaskService.cs:140`. `AppDbContext` turns it into a soft delete (test `Delete_SoftDeletes_...`, and SQL shows `IsDeleted = 1`) |
| Magic numbers | grep | ✅ only comment references. Lengths use `TodoTask.TitleMaxLength` / `DescriptionMaxLength` |
| Async suffix / one public type per file | grep + count | ✅ (`TaskWriteRequestValidator<T>` is one public generic type) |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/06-task-api` from `main` (2c6573a); 4 commits, all `[Task 06] ...` |
| Remote / tracked junk / packages / migrations | `git remote -v`, `git ls-files`, diff | ✅ no remote, nothing tracked, no csproj or migration changes |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| All six endpoints work in Swagger | ✅ (API) / ⚠️ UI click-through not done | All three routes are in `swagger.json`. With a JWT: `GET` list `200`, `GET /{id}` `200`/`404`, `POST` `201` + `Location`, `PUT` `200`, `PATCH /{id}/complete` `200`, `DELETE` `204` |
| `GET /tasks` with 1,000 seeded tasks < 500 ms (NFR-1) | ✅ | 1,000 tasks inserted with `sqlcmd` for one user; 6 runs 0.011–0.025 s; `?status=pending` 0.047 s |
| All tests in this task pass | ✅ | 107/107 |

Steps / tests check:
- DTO fields match the task file (`TaskListItemDto`, `TaskDetailDto`, requests, `SetCompletionRequest`, `TaskStatusFilter`). ✅
- Category must belong to the user → `400` (`OwnCategory_Passes_ButAnotherUsersOrDeletedCategory_IsRejected`; real API `errors.categoryId`). ✅
- Status filter and order (`GetAll_FiltersByStatus_AndOrdersPendingFirstThenDueDateThenNewest`, `GetAll_TasksWithoutDueDate_AreOrderedNewestFirst`); invalid status → `400` (`GetAll_UndefinedStatus_Returns400`, plus real API `bogus`/`7`). ✅
- `isOverdue` in one shared helper (`TaskRules`; `TaskRulesTests`, `IsOverdue_IsTrueOnlyForPastDueNotCompletedTasks`). ✅
- Create with only a title → Medium, not completed (`Create_WithOnlyTitle_...`). ✅
- Title empty / > 200 rejected (`EmptyTitle_IsRejected`, `TitleOver200_OrDescriptionOver2000_IsRejected`). ✅
- Complete sets `CompletedAt`; reopen clears it (`SetCompletion_SetsCompletedAt_AndReopenClearsIt`). ✅
- Soft-deleted tasks are hidden (`Delete_SoftDeletes_AndTaskIsHiddenFromListAndDetail`). ✅
- User B gets not-found on get/update/complete/delete (`UserB_GetsNotFound_ForUserATask_...`; real API all `404`). ✅
- After a category delete, its tasks show `categoryId = null` (`AfterCategoryDelete_TasksShowNoCategory`). ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–05 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1 | ✅ | Endpoints and fields match ARCHITECTURE §6 and the task file |
| G-SCOPE-2 | ✅ | `SubTaskDto` (fields as written in Task 07) is defined early only as the element type of `TaskDetailDto.subTasks`. The list stays empty until Task 07. `subTaskTotal`/`subTaskCompleted` are DTO fields of this task and already count non-deleted subtasks |
| G-SCOPE-3 | ✅ | No Phase 2 work |
| G-SCOPE-4 | ✅ | Only `Program.cs` changed outside this task (DI registration) |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1, 2 | ✅ | No secrets, no logging |
| G-SEC-3 | ✅ | `UserId` only from the JWT; `categoryId` in the body is checked against the user's own categories |
| G-SEC-4 | ✅ | EF LINQ only (the `sqlcmd` seeding was a manual verification step, not code) |
| G-SEC-5 | N/A | No frontend |
| G-SEC-6 | ✅ | No token → `401` |
| G-SEC-7 | ✅ | `400` ValidationProblemDetails / `404` ProblemDetails, no internals |
| G-SEC-8 | ✅ | Another user's task → `404` (unit + real API) |
| G-CODE-1, 2 | ✅ | 0 warnings; format clean |
| G-CODE-3 | ✅ | Controller only maps service results to HTTP |
| G-CODE-4 | ✅ | DTOs only |
| G-CODE-5, 6, 7 | ✅ | Naming, one type per file, no dead code |
| G-CODE-8 | ✅ | Entity length constants; no business-rule literals |
| G-TEST-1 | ✅ | 107/107 |
| G-TEST-2 | ✅ | Every `ITaskService` method has a happy path and a not-found test (`UnknownId_IsNotFound_ForEveryOperation`) |
| G-TEST-3 | ✅ | `UserB_GetsNotFound_ForUserATask_OnGetUpdateCompleteAndDelete`, plus the validator's other-user category test |
| G-TEST-4 | N/A | From Task 09 |
| G-DB-1, 2 | ✅ / N/A | No migrations |
| G-DB-3 | ✅ | Soft delete only |
| G-DB-4 | ✅ | `CompletedAt` from `UtcNow`; `+05:30` due date stored as UTC (`04:30Z`); tests check `Kind == Utc` |
| G-DESTRUCT-1 to 4 | ✅ | Nothing dropped or deleted. Verification data was added (see below) |

## Issues to fix (FAIL only)
None.

## Not verified
- **Swagger UI click-through:** routes are in Swagger, and the same calls passed with `curl` and a JWT. Client: try the six task endpoints from `/swagger` after "Authorize".
- **Subtasks of a deleted task:** deleting a task does not mark its subtasks `IsDeleted` (neither Task 06 nor Task 07 asks for this). They cannot be reached, because subtasks are only read through their (filtered) parent task. Decide whether this matters for a later purge (BRD §9.6).
- **Verification data in `TodoAppDb`:** users `task06-a/b/perf-…@example.test`; the perf user has **1,000 seeded tasks** ("Perf task N"), inserted with `sqlcmd` for the NFR-1 check. Tell me if you want them removed (that is a delete, so it needs your OK).

## Files changed outside this task's scope
- `backend/TodoApp.Api/Program.cs` — registers `ITaskService` (required).
- `plans/00-INDEX.md` — Task 06 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
