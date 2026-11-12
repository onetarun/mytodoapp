# Task 15 — Frontend: Dashboard & Notification Bell

**Depends on:** 14
**References:** FR-17 to FR-22, NFR-24 · US-6, US-7
**Goal:** A dashboard with live counts and upcoming tasks, and a header bell that alerts the user about overdue and due-soon tasks.

## Steps

### Dashboard (`/dashboard`)
- [x] `DashboardService.getSummary()` — sends the browser time zone (`Intl.DateTimeFormat().resolvedOptions().timeZone`) as `tz`.
- [x] Five stat cards: **Total, Completed, Pending, Overdue, Due Today** (FR-17). Overdue card uses red + icon.
- [x] Clicking a card goes to the task list with the matching filter where possible (Pending → `?status=pending`, Completed → `?status=completed`). *(Total → `/tasks`; Overdue and Due Today have no matching list filter, so they are not links.)*
- [x] "Upcoming (next 7 days)" list with title, due date and priority; click opens the task (FR-18).
- [x] Quick-add box on the dashboard too (optional, nice for NFR-21).
- [x] Empty state for a brand-new user.

### Notification bell (header)
- [x] `NotificationService` with a `notifications` signal.
- [x] Fetch on login/page load and then **every 5 minutes** (RxJS `timer(0, 300000)`), stopping on logout (FR-22).
- [x] Also refresh after any task create/update/complete/delete so the count stays correct.
- [x] Bell icon with a badge showing the count; hide the badge when 0 (FR-19).
- [x] `aria-label`, e.g. "Notifications, 3 items".
- [x] Click opens a dropdown: "Overdue" group (red + icon) and "Due within 24 hours" group, each item linking to `/tasks/:id` (FR-20). Close with Esc or a click outside.
- [x] Empty state: "You're all caught up".

## Definition of Done

- [x] Dashboard counts match the task list for a test user with a mix of tasks. *(User with 7 tasks: overdue ×2, due in 30 min, in 3 days, in 10 days, no due date, completed. Dashboard Total 7 / Completed 1 / Pending 6 / Overdue 2 / Due Today 2 = `GET /tasks?status=all|completed|pending` 7 / 1 / 6, `isOverdue` 2, pending due on the local day 2. Upcoming shows only the 30-min and 3-day tasks. Dashboard quick add → Total 8→9 with "Task added." toast. A brand-new user sees "No tasks yet".)*
- [x] Creating a task due in 2 hours increases the bell count without a page reload. *(Task list "Add details" with due 2 h ahead → bell "Notifications, 3 items" → "4 items", badge 4, a `window` marker set before the action was still there. Completing/reopening a task changed it 4→3→4 with exactly one `/notifications` request per change.)*
- [x] Clicking a notification opens that task. *(Menu groups: Overdue (red border + warning icon) with 2 items, "Due within 24 hours" with 1; ArrowDown + Enter on "Submit report" → `/tasks/2014`. Esc and a click outside close the menu; 0 items shows "You're all caught up" and hides the badge.)*
- [x] Polling stops after logout (no 401s in the network tab). *(Resource timing: page load 01:55:50Z → poll at 02:00:51Z (5 min). Logout at 02:03:40Z, watched until 02:09:17Z (past the next poll time 02:05:52Z): 0 API requests; API log has no 401. Logging in again fetched the bell at once.)*
