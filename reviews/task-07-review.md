# Review — Task 07: SubTask (Checklist) API

**Date:** 2026-11-11 · **Branch:** task/07-subtask-api · **Verdict:** ✅ PASS

## Summary
This task adds `ISubTaskService`/`SubTaskService` and `SubTasksController` (`POST`, `PUT`, `PATCH .../complete`, `DELETE` under `/api/v1/tasks/{taskId}/subtasks`), the subtask request DTOs and their validators. Every method first checks the parent task through the filtered `TodoTasks` set, then looks up the subtask by id **and** task id. `TaskService.GetByIdAsync` now returns the non-deleted subtasks ordered by `SortOrder`, and the list shows `x/y` progress. There are 17 new tests (124 in total), and everything was checked against the real API and SQL Server.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | csproj inspection (unchanged) | ✅ |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 124 passed, 0 failed, 0 skipped |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none |
| Secrets / logging | grep changed files | ✅ none |
| `UserId` from request | grep | ✅ none. `SubTaskService` never touches `UserId`; ownership comes through the parent task's filter |
| Raw SQL / `IgnoreQueryFilters` in API code | grep | ✅ none |
| `[AllowAnonymous]` | grep | ✅ only `AuthController`, `HealthController` |
| DbContext in controllers / entity return types | grep | ✅ none |
| TODO / FIXME / `Skip =` / `DateTime.Now` / `RemoveRange` | grep | ✅ none |
| `.Remove(` on user data | grep | ✅ `SubTaskService.cs:76` (and the unchanged `TaskService.cs:142`), both turned into soft deletes by `AppDbContext`. Test `DeletedSubTask_IsSoftDeleted_...`, and SQL shows `IsDeleted = 1` |
| Magic numbers | grep | ✅ only comment references; `SubTask.TitleMaxLength` used |
| Async suffix / one public type per file | grep + count | ✅ |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/07-subtask-api` from `main` (743803e); 4 commits, all `[Task 07] ...` |
| Remote / tracked junk / packages / migrations | `git remote -v`, `git ls-files`, diff | ✅ no remote, nothing tracked, no csproj or migration changes |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| All four endpoints work in Swagger | ✅ (API) / ⚠️ UI click-through not done | Three routes are in `swagger.json`. With a JWT: `POST` `201` + `Location`, `PUT` `200`, `PATCH` `200`, `DELETE` `204` (repeat `404`) |
| `GET /tasks/{id}` shows subtasks; `GET /tasks` shows progress | ✅ | Detail lists `Book hostel` (2), `Pack` (3) in sort order. List shows `subTaskTotal 3`, `subTaskCompleted 2`, and after a delete `2` / `1` |
| All tests in this task pass | ✅ | 124/124 |

Steps / tests check:
- DTOs and validators (title required, max 200; `SetCompletionRequest` reused). ✅
- Parent task checked through the filtered set before every operation (`UnknownOrDeletedParentTask_ReturnsNotFound`). ✅
- Sort orders 1, 2, 3 (`AddingThreeSubTasks_GivesSortOrders1To3_...`; real API). ✅
- 2 of 3 ticked → `2/3` (`TickingTwoOfThree_GivesProgress2Of3_InListAndDetail`). ✅
- Deleted subtask excluded from detail and counts (`DeletedSubTask_IsSoftDeleted_AndExcludedFromDetailAndProgress`). ✅
- User B cannot add/edit/complete/delete (`UserB_GetsNotFound_OnUserATask_...`; real API all `404`). ✅
- Subtask id under a different task → not-found (`SubTaskOfAnotherTask_ReturnsNotFound`; real API `404`). ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–06 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1, 2 | ✅ | Matches the task file and ARCHITECTURE §6 (four subtask endpoints) |
| G-SCOPE-3 | ✅ | No reordering/drag-and-drop or other extras |
| G-SCOPE-4 | ✅ | `TaskService.cs` and `SubTaskDto.cs` (Task 06) changed as this task's step requires; `Program.cs` DI registration |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1, 2 | ✅ | No secrets, no logging |
| G-SEC-3 | ✅ | No `UserId` input; `taskId`/`subId` are resource ids checked against the user's own tasks |
| G-SEC-4 | ✅ | EF LINQ only |
| G-SEC-5 | N/A | No frontend |
| G-SEC-6 | ✅ | No token → `401` |
| G-SEC-7 | ✅ | `400` / `404` ProblemDetails, no internals |
| G-SEC-8 | ✅ | Another user's task → `404` on every subtask endpoint |
| G-CODE-1 to 8 | ✅ | 0 warnings, format clean, controller → service only, DTOs only, naming, one type per file, no dead code (the Task 06 `NoSubTasks` placeholder was removed), no literals |
| G-TEST-1 | ✅ | 124/124 |
| G-TEST-2 | ✅ | `CreateAsync`, `UpdateAsync`, `SetCompletionAsync`, `DeleteAsync` each have a happy path and not-found tests |
| G-TEST-3 | ✅ | `UserB_GetsNotFound_OnUserATask_ForAddEditCompleteAndDelete` |
| G-TEST-4 | N/A | From Task 09 |
| G-DB-1, 2 | ✅ / N/A | No migrations |
| G-DB-3 | ✅ | Soft delete only |
| G-DB-4 | ✅ | `DeletedAt` from `UtcNow` (`AppDbContext`) |
| G-DESTRUCT-1 to 4 | ✅ | Nothing dropped or deleted |

## Issues to fix (FAIL only)
None.

## Not verified
- **Swagger UI click-through:** routes are in Swagger, and the same calls passed with `curl` and a JWT.
- **Subtasks of a deleted task** stay `IsDeleted = 0`, but they cannot be reached (noted in the Task 06 review). Neither task asks for a cascade.
- **Verification data in `TodoAppDb`:** users `task07-a/b-…@example.test` with two tasks and three subtasks.

## Files changed outside this task's scope
- `backend/TodoApp.Api/Services/TaskService.cs` — fills `TaskDetailDto.subTasks` (required by this task's step); removed the empty-list placeholder.
- `backend/TodoApp.Api/DTOs/Tasks/SubTaskDto.cs` — doc comment only (the type was added in Task 06).
- `backend/TodoApp.Api/Program.cs` — registers `ISubTaskService`.
- `plans/00-INDEX.md` — Task 07 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
