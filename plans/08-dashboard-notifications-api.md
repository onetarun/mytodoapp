# Task 08 — Dashboard & Notifications API

**Depends on:** 07
**References:** FR-17 to FR-22, NFR-1 · ARCHITECTURE §6 (computed on request)
**Goal:** The API returns dashboard stats and the bell's notification list, computed live from the user's tasks.

## Steps

### DTOs
- [x] `DashboardSummaryDto` — `total`, `completed`, `pending`, `overdue`, `dueToday`, `upcoming[]` (`TaskListItemDto`, next 7 days, not completed, max 10, ordered by due date).
- [x] `NotificationDto` — `taskId`, `title`, `dueDate`, `type` (`Overdue` | `DueSoon`).
- [x] `NotificationsResponse` — `count`, `items[]`.

### Time rules (decide once, reuse everywhere)
- [x] **Overdue:** not completed and `DueDate < UtcNow` (same helper as Task 06).
- [x] **Due soon:** not completed and `UtcNow ≤ DueDate ≤ UtcNow + 24h`.
- [x] **Due today:** not completed and `DueDate` falls within the user's **local** calendar day. The client sends its UTC offset or time zone (e.g. `?tz=Asia/Kolkata`) so the server can compute day boundaries; default to UTC if missing.

### Services
- [x] `IDashboardService.GetSummaryAsync(tz)` — counts with as few queries as possible (one grouped query preferred), `AsNoTracking()`.
- [x] `INotificationService.GetAsync()` — overdue first, then due soon; ordered by due date.

### Controllers
- [x] `GET /api/v1/dashboard/summary?tz=...` → `200`.
- [x] `GET /api/v1/notifications` → `200`.

### Tests
- [x] Seed a known set (completed, pending, overdue, due today, due in 3 days, due in 10 days, no due date, soft-deleted) and assert every count exactly.
- [x] `upcoming` contains only not-completed tasks due within 7 days.
- [x] Notifications include overdue and due-within-24h only; completed and deleted tasks are excluded.
- [x] Another user's tasks never affect the counts.

## Definition of Done

- [x] Both endpoints work in Swagger and the numbers match a manual check in SSMS.
- [x] Both respond in < 500 ms with 1,000 tasks (NFR-1).
- [x] All tests in this task pass.

## Notes

- **Time rules:** they live in `Services/TaskRules`, which already held the Task 06 overdue helper. Added: `DueSoonHours = 24`, `UpcomingDays = 7`, `UpcomingMaxItems = 10`, `IsDueSoon`, `NeedsNotificationExpression` (overdue or due soon), `IsDueTodayExpression`, `IsUpcomingExpression`, and `LocalDayRange(utcNow, timeZone)`. `LocalDayRange` turns the user's local calendar day into a UTC range and handles a DST jump at midnight.
- **Time zone:** `?tz=` takes an IANA id (e.g. `Asia/Kolkata`) and is resolved with `TimeZoneInfo.TryFindSystemTimeZoneById`. Missing → UTC; unknown → `400 errors.tz`.
- **Dashboard queries:** one grouped query (`Total`, `Completed`), one `CountAsync` each for overdue and due today (so the shared rule expressions are used as they are), and one upcoming query. All use `AsNoTracking()`; `Pending = Total − Completed`.
- **Shared projection:** `TaskProjections.ListItem` / `WithOverdue` is now used by `TaskService.GetAllAsync` and the dashboard's upcoming list.
- **Notifications:** ordered by due date, which puts every overdue task before every due-soon task. `count` = number of items.
- **Clock:** `TimeProvider` (BCL, registered as `TimeProvider.System`) is injected into both services, so tests use a fixed clock (`FixedTimeProvider` in the test project).

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Build / format | `dotnet build --no-incremental`, `dotnet format --verify-no-changes` | 0 warnings, 0 errors; format clean |
| Tests | `dotnet test` | 137 passed (13 new), 0 failed, 0 skipped |
| Endpoints (real API + SQL Server) | `curl` | no token `401`; empty user → all zero; bad `tz` → `400` |
| Numbers vs. manual SQL | 1,000 tasks seeded with `sqlcmd` for one user (20 soft deleted), then API vs. a hand-written `sqlcmd` query with the same rules | API `total 980, completed 180, pending 800, overdue 300, dueToday 15` (UTC and Asia/Kolkata), upcoming 10 items (101 match, capped at 10); notifications `count 315` = 300 Overdue + 15 DueSoon. SQL: `980 / 180 / 800 / 300 / 15 / 15 / 101 / 315 / 15`, all equal |
| NFR-1 | `curl -w %{time_total}`, 6 runs each with 1,000 tasks | summary 0.009–0.014 s; notifications 0.005–0.031 s |
| Swagger | `/swagger/v1/swagger.json` | `/api/v1/dashboard/summary`, `/api/v1/notifications` listed. The UI click-through and an SSMS GUI check were not done (`sqlcmd` used instead) |
