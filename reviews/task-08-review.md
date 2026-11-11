# Review — Task 08: Dashboard & Notifications API

**Date:** 2026-11-11 · **Branch:** task/08-dashboard-notifications-api · **Verdict:** ✅ PASS

## Summary
This task adds `GET /api/v1/dashboard/summary?tz=...` (Total / Completed / Pending / Overdue / Due Today, plus up to 10 upcoming tasks for the next 7 days) and `GET /api/v1/notifications` (overdue and due-within-24h tasks, overdue first). Both are computed live from the current user's tasks. All time rules are in the shared `TaskRules` class, which already held the Task 06 overdue helper. "Due today" uses the user's IANA time zone. There are 13 new tests (137 in total) with a fixed clock. On the real API with 1,000 tasks, every number matched a hand-written SQL query, and both endpoints answered in ≤ 31 ms.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | csproj inspection (unchanged) | ✅ |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 137 passed, 0 failed, 0 skipped |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none |
| Secrets / logging | grep changed files | ✅ none |
| `UserId` from request | grep | ✅ none. The only input is `tz` (a time-zone id) |
| Raw SQL / `IgnoreQueryFilters` in API code | grep | ✅ none |
| `[AllowAnonymous]` | grep | ✅ only `AuthController`, `HealthController` |
| DbContext in controllers / entity return types | grep | ✅ none; DTOs only |
| TODO / FIXME / `Skip =` / `DateTime.Now` / `RemoveRange` | grep | ✅ none. Services read time from `TimeProvider` |
| `.Remove(` | grep | ✅ only the unchanged soft delete in `TaskService` |
| Magic numbers (`24`, `7`, `10`) | grep | ✅ only as named constants `TaskRules.DueSoonHours`, `UpcomingDays`, `UpcomingMaxItems` |
| Async suffix / one public type per file | grep + count | ✅ |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/08-dashboard-notifications-api` from `main` (bda878a); 4 commits, all `[Task 08] ...` |
| Remote / tracked junk / packages / migrations | `git remote -v`, `git ls-files`, diff | ✅ no remote, nothing tracked, no csproj or migration changes (`TimeProvider` is part of .NET) |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Both endpoints work in Swagger and the numbers match a manual check in SSMS | ✅ (API + `sqlcmd`) / ⚠️ Swagger UI and SSMS GUI not opened | Both routes are in `swagger.json`. With 1,000 seeded tasks, the API returned `980 / 180 / 800 / 300 / 15` (dueToday 15 in UTC and in Asia/Kolkata), and notifications returned `315` (300 Overdue + 15 DueSoon). A hand-written `sqlcmd` query with the same rules returned `980 / 180 / 800 / 300 / 15 / 15`, `315`, `15`; upcoming candidates 101, capped to 10 |
| Both respond in < 500 ms with 1,000 tasks | ✅ | summary 0.009–0.014 s, notifications 0.005–0.031 s (6 runs each) |
| All tests in this task pass | ✅ | 137/137 |

Steps / tests check:
- DTOs: `DashboardSummaryDto`, `NotificationDto` (`Overdue` | `DueSoon`), `NotificationsResponse` as specified. ✅
- Time rules: overdue reuses the Task 06 helper; due soon `now ≤ due ≤ now + 24h`; due today uses the local calendar day from `?tz=`, defaulting to UTC (`Summary_DueToday_UsesTheUsersLocalDay`, with Kolkata 1 vs. Los Angeles 2). ✅
- Few queries, `AsNoTracking()`: one grouped query plus two `CountAsync` that use the shared rule expressions, plus the upcoming query. ✅ (see note under Not verified)
- Notifications: overdue first, then due soon, by due date. ✅
- Known set, every count exact (`Summary_InUtc_CountsEveryRuleExactly`). ✅
- `upcoming` only not-completed within 7 days (same test), max 10 ordered (`Summary_Upcoming_IsLimitedTo10_AndOrderedByDueDate`). ✅
- Notifications only overdue/≤24h; completed and deleted excluded (`Notifications_IncludeOnlyOverdueAndDueWithin24h_OverdueFirst`, `..._DueExactlyIn24h_...`). ✅
- Another user's tasks never affect counts (`Summary_OtherUsersTasks_NeverAffectTheCounts`, `Notifications_ForOtherUser_ShowOnlyTheirOwnTasks`). ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–07 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1, 2 | ✅ | Endpoints, fields and rules as in the task file / ARCHITECTURE §6 ("computed on request"). `400` for an unknown `tz` is input validation (NFR-13) |
| G-SCOPE-3 | ✅ | No email/push reminders (in-app only) |
| G-SCOPE-4 | ✅ | `TaskRules.cs` extended (the task says to reuse it); `TaskService.cs` now uses the shared `TaskProjections.ListItem` (same output, tests unchanged); `Program.cs` DI |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1 to 4 | ✅ | No secrets, no logging, no `UserId` input, EF LINQ only |
| G-SEC-5 | N/A | No frontend |
| G-SEC-6 | ✅ | No token → `401` on both |
| G-SEC-7 | ✅ | `400` ValidationProblemDetails for a bad `tz`; no internals |
| G-SEC-8 | ✅ | Only the user's own tasks are counted or listed |
| G-CODE-1 to 7 | ✅ | 0 warnings, format clean, controllers → services, DTOs only, naming, one type per file, no dead code |
| G-CODE-8 | ✅ | 24 h / 7 days / 10 items are named constants in `TaskRules` |
| G-TEST-1 | ✅ | 137/137 |
| G-TEST-2 | ✅ | `GetSummaryAsync`: full known set + empty user + local time zone. `GetAsync`: happy path + boundary/exclusions + other user. Controller: bad `tz` → `400` |
| G-TEST-3 | ✅ | `Summary_OtherUsersTasks_NeverAffectTheCounts`, `Notifications_ForOtherUser_ShowOnlyTheirOwnTasks` |
| G-TEST-4 | N/A | From Task 09 |
| G-DB-1 to 3 | ✅ / N/A | Read-only; no migrations |
| G-DB-4 | ✅ | All comparisons in UTC; only the day boundaries are converted from the user's zone |
| G-DESTRUCT-1 to 4 | ✅ | Nothing dropped or deleted |

## Issues to fix (FAIL only)
None.

## Not verified
- **Swagger UI click-through and SSMS GUI:** numbers were checked with `curl` and `sqlcmd`, not in the Swagger UI or SSMS window. Client: open both endpoints in `/swagger` and compare with SSMS if you like.
- **"One grouped query preferred":** the summary uses 4 small queries: grouped Total/Completed, Overdue and Due Today counts through the shared rule expressions, and the upcoming list. This keeps one definition of each rule; it measured ~10 ms with 1,000 tasks.
- **DST edge:** `LocalDayRange` handles a missing local midnight (DST jump). Neither India nor UTC has DST, and this path is not covered by a test.
- **Verification data in `TodoAppDb`:** user `task08-perf-…@example.test` with **1,000 seeded tasks** ("Perf8 task N"), the second perf set after Task 06. Tell me if you want test data cleaned up (that needs your OK).

## Files changed outside this task's scope
- `backend/TodoApp.Api/Services/TaskRules.cs` (Task 06) — new shared rules and constants (required: "decide once, reuse everywhere").
- `backend/TodoApp.Api/Services/TaskService.cs` (Task 06) — uses `TaskProjections.ListItem` / `WithOverdue` instead of its own copy.
- `backend/TodoApp.Api/Program.cs` — registers `TimeProvider.System`, `IDashboardService`, `INotificationService`.
- `plans/00-INDEX.md` — Task 08 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
