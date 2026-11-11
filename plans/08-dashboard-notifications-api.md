# Task 08 — Dashboard & Notifications API

**Depends on:** 07
**References:** FR-17 to FR-22, NFR-1 · ARCHITECTURE §6 (computed on request)
**Goal:** The API returns dashboard stats and the bell's notification list, computed live from the user's tasks.

## Steps

### DTOs
- [ ] `DashboardSummaryDto` — `total`, `completed`, `pending`, `overdue`, `dueToday`, `upcoming[]` (`TaskListItemDto`, next 7 days, not completed, max 10, ordered by due date).
- [ ] `NotificationDto` — `taskId`, `title`, `dueDate`, `type` (`Overdue` | `DueSoon`).
- [ ] `NotificationsResponse` — `count`, `items[]`.

### Time rules (decide once, reuse everywhere)
- [ ] **Overdue:** not completed and `DueDate < UtcNow` (same helper as Task 06).
- [ ] **Due soon:** not completed and `UtcNow ≤ DueDate ≤ UtcNow + 24h`.
- [ ] **Due today:** not completed and `DueDate` falls within the user's **local** calendar day. The client sends its UTC offset or time zone (e.g. `?tz=Asia/Kolkata`) so the server can compute day boundaries; default to UTC if missing.

### Services
- [ ] `IDashboardService.GetSummaryAsync(tz)` — counts with as few queries as possible (one grouped query preferred), `AsNoTracking()`.
- [ ] `INotificationService.GetAsync()` — overdue first, then due soon; ordered by due date.

### Controllers
- [ ] `GET /api/v1/dashboard/summary?tz=...` → `200`.
- [ ] `GET /api/v1/notifications` → `200`.

### Tests
- [ ] Seed a known set (completed, pending, overdue, due today, due in 3 days, due in 10 days, no due date, soft-deleted) and assert every count exactly.
- [ ] `upcoming` contains only not-completed tasks due within 7 days.
- [ ] Notifications include overdue and due-within-24h only; completed and deleted tasks are excluded.
- [ ] Another user's tasks never affect the counts.

## Definition of Done

- [ ] Both endpoints work in Swagger and the numbers match a manual check in SSMS.
- [ ] Both respond in < 500 ms with 1,000 tasks (NFR-1).
- [ ] All tests in this task pass.
