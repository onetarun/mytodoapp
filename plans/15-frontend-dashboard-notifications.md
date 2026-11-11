# Task 15 — Frontend: Dashboard & Notification Bell

**Depends on:** 14
**References:** FR-17 to FR-22, NFR-24 · US-6, US-7
**Goal:** A dashboard with live counts and upcoming tasks, and a header bell that alerts the user about overdue and due-soon tasks.

## Steps

### Dashboard (`/dashboard`)
- [ ] `DashboardService.getSummary()` — sends the browser time zone (`Intl.DateTimeFormat().resolvedOptions().timeZone`) as `tz`.
- [ ] Five stat cards: **Total, Completed, Pending, Overdue, Due Today** (FR-17). Overdue card uses red + icon.
- [ ] Clicking a card goes to the task list with the matching filter where possible (Pending → `?status=pending`, Completed → `?status=completed`).
- [ ] "Upcoming (next 7 days)" list with title, due date and priority; click opens the task (FR-18).
- [ ] Quick-add box on the dashboard too (optional, nice for NFR-21).
- [ ] Empty state for a brand-new user.

### Notification bell (header)
- [ ] `NotificationService` with a `notifications` signal.
- [ ] Fetch on login/page load and then **every 5 minutes** (RxJS `timer(0, 300000)`), stopping on logout (FR-22).
- [ ] Also refresh after any task create/update/complete/delete so the count stays correct.
- [ ] Bell icon with a badge showing the count; hide the badge when 0 (FR-19).
- [ ] `aria-label`, e.g. "Notifications, 3 items".
- [ ] Click opens a dropdown: "Overdue" group (red + icon) and "Due within 24 hours" group, each item linking to `/tasks/:id` (FR-20). Close with Esc or a click outside.
- [ ] Empty state: "You're all caught up".

## Definition of Done

- [ ] Dashboard counts match the task list for a test user with a mix of tasks.
- [ ] Creating a task due in 2 hours increases the bell count without a page reload.
- [ ] Clicking a notification opens that task.
- [ ] Polling stops after logout (no 401s in the network tab).
