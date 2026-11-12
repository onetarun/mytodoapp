# Review — Task 15: Frontend: Dashboard & Notification Bell

**Date:** 2026-11-12 · **Branch:** task/15-frontend-dashboard-notifications · **Verdict:** ✅ PASS

## Summary
The dashboard now shows five stat cards (Total, Completed, Pending, Overdue in red with an icon, Due Today), the upcoming list for the next 7 days, a quick-add box and an empty state for new users. The header bell polls `/notifications` on load and every 5 minutes, refreshes after every task change, shows a badge and the accessible count, and opens a menu with "Overdue" and "Due within 24 hours" groups. Everything was tested in the browser against the real API, including a 5-minute poll and an 11-minute check that polling stops after logout. No backend code changed.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | ✅ "All files pass linting." |
| Prettier | `npx prettier --check "src/**/*.{ts,html,scss}"` | ✅ "All matched files use Prettier code style!" |
| Strict mode | `tsconfig.json` | ✅ `"strict": true`, `"strictTemplates": true` (unchanged) |
| Build | `npx ng build` | ✅ complete, no warnings; initial 556.70 kB raw / 132.47 kB transfer (under the approved 650 kB warning budget) |
| Backend build/format/tests/coverage | — | N/A: no `backend/` changes in `git diff main...HEAD` |
| Code searches (changed files) | Grep: `innerHTML`, `outerHTML`, `bypassSecurityTrust`, `console.log`, `TODO`, `FIXME`, `xit(`/`fit(`/`fdescribe`/`xdescribe`, `localStorage` | ✅ no matches |
| Comments | Grep `^\s*//` | ✅ 3 explanatory comments, no commented-out code |
| Magic numbers | Grep `5`, `15`, `24`, `60`, `100`, `300000` | ✅ Poll interval is the named constant `NOTIFICATION_POLL_INTERVAL_MS = 5 * 60 * 1000`; "24 hours" only in UI text; the due-soon window itself is computed by the API |
| Packages | `git diff main...HEAD -- package.json package-lock.json` | ✅ no changes (`MatBadge`, `MatMenu` come from the approved `@angular/material`) |
| Tracked build output | `git ls-files` | ✅ none |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Step: `DashboardService.getSummary()` sends `tz` | ✅ | `features/dashboard/dashboard.service.ts`; browser zone `Asia/Kolkata` → "Due Today" matched the local day |
| Step: five stat cards, Overdue red + icon | ✅ | `dashboard-page.component.html/.scss`; Overdue card has class `danger`, `warning` icon and the "Overdue" label (NFR-24) |
| Step: card → list with matching filter where possible | ✅ | DOM: Total → `/tasks`, Completed → `/tasks?status=completed`, Pending → `/tasks?status=pending`; Overdue/Due Today have no list filter, so they are plain cards (noted in the task file) |
| Step: Upcoming (next 7 days) with title, due date, priority; click opens task | ✅ | List showed "Call plumber" (30 min) and "Dentist visit" (3 days), not the 10-day task; click → `/tasks/2015` |
| Step: quick add on dashboard (optional) | ✅ | "Water plants" + Enter → toast "Task added.", Total 8 → 9 |
| Step: empty state for a new user | ✅ | New user: "No tasks yet — Add your first task above, or open the task list." |
| Step: `NotificationService` with a `notifications` signal | ✅ | `features/notifications/notification.service.ts` (`notifications`, `count`, `overdue`, `dueSoon`) |
| Step: fetch on load + every 5 min, stop on logout | ✅ | `timer(0, NOTIFICATION_POLL_INTERVAL_MS)`; page load 01:55:50Z → poll 02:00:51Z; `stop()` from the bell's `DestroyRef` and from `logout()` |
| Step: refresh after create/update/complete/delete | ✅ | `TaskService` pipes `refreshBell()` on all four; one `/notifications` request per change (resource timing) |
| Step: bell badge, hidden at 0 | ✅ | Badge "3"/"4"; `mat-badge-hidden` at 0 |
| Step: `aria-label` "Notifications, N items" | ✅ | "Notifications, 0 items" / "3 items" / "4 items"; icon and badge are `aria-hidden` |
| Step: dropdown groups, links, close with Esc / outside click | ✅ | Overdue group (red border + warning icon), "Due within 24 hours" group; links `/tasks/2013`, `/tasks/2014`, `/tasks/2015`; Esc and a backdrop click closed it; focus went back to the bell |
| Step: empty "You're all caught up" | ✅ | Seen for a user with no due tasks |
| DoD: dashboard counts match the task list | ✅ | 7/1/6/2/2 vs `GET /tasks?status=all|completed|pending` = 7/1/6, `isOverdue` = 2, pending due today = 2 |
| DoD: task due in 2 h increases bell count without reload | ✅ | 3 → 4 items; a `window` marker set before the action was still present |
| DoD: clicking a notification opens that task | ✅ | ArrowDown + Enter on "Submit report" → `/tasks/2014`, menu closed |
| DoD: polling stops after logout, no 401s | ✅ | Logout 02:03:40Z, watched to 02:09:17Z (past the next poll 02:05:52Z): 0 API requests; API log has no `401` |

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–14 ✅ in the index |
| G-PROC-2 | ✅ | Client ran `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 to 7 | ✅ | No blockers |
| G-SCOPE-1, 2, 3 | ✅ | Only FR-17 to FR-22, NFR-24 and task file items (quick add is listed as optional in the task file); no Phase 2 work |
| G-SCOPE-4 | ✅ | Earlier files changed only where needed (listed below) |
| G-GIT-1 | ✅ | `task/15-frontend-dashboard-notifications` from `main` at `ad9cf98` |
| G-GIT-2 | ✅ | All 3 commits start with `[Task 15]` |
| G-GIT-3 | ✅ | Not merged; waiting for client approval |
| G-GIT-4 | ✅ | No remote / upstream |
| G-GIT-5 | ✅ | Nothing ignored is tracked; the temporary `.claude/launch.json` used for testing was removed and never committed |
| G-DEP-1 to 3 | ✅ | No package changes |
| G-SEC-1, 2 | ✅ | No secrets, no logging |
| G-SEC-3 | ✅ | Only `tz` is sent; no `UserId` |
| G-SEC-4 | N/A | No backend changes |
| G-SEC-5 | ✅ | Titles shown only through `{{ }}` |
| G-SEC-6 to 8 | N/A | Backend unchanged (verified in Task 08); the bell never calls the API without a valid session |
| G-CODE-1, 2 | ✅ | Build, lint, Prettier clean |
| G-CODE-3, 4 | N/A | Backend only |
| G-CODE-5, 6 | ✅ | kebab-case files; one public class per file (the `StatCard` interface is file-private) |
| G-CODE-7 | ✅ | No dead code |
| G-CODE-8 | ✅ | Named poll constant; business windows (24 h, 7 days) come from the API |
| G-TEST-1 to 4 | N/A | No frontend tests in v1 (same as Tasks 10–14); backend unchanged |
| G-DB-1 to 4 | N/A | No database or date-storage changes (dates are shown from UTC in local time) |
| G-DESTRUCT-1 to 4 | ✅ | Nothing destructive. `features/notifications/.gitkeep` (Task 10) was left in place, not deleted |

## Issues to fix (FAIL only)
None.

## Not verified
- **Screen reader:** labels checked in the DOM only (Task 16). The menu group headings are `aria-hidden`; each item says "Overdue, was due …" or "Due within 24 hours, due …" so the group is still announced.
- **Mobile widths (360 px):** stat cards use an `auto-fit` grid and the upcoming rows wrap, but the layout was not checked at 360 px (Task 16). The browser pane was small during testing, so screenshots were only partial; checks used the DOM.
- **Hidden-tab timers:** Chrome may delay timers in background tabs, so a poll can arrive a little after 5 minutes when the tab is hidden. On-load and after-change refreshes are not affected.
- **Initial bundle growth:** 502.66 kB → 556.70 kB raw (132.47 kB transfer), because the bell (menu, badge) is in the always-loaded header. Still under the 650 kB warning budget.
- **Test data in `TodoAppDb`:** user `task15-dashboard@example.test` with 9 tasks (2 overdue, 1 completed).

## Files changed outside this task's scope
- `frontend/todo-app/src/app/core/layout/app-shell/app-shell.component.{html,ts}` (Task 10) — the disabled placeholder bell was replaced by `<app-notification-bell>`; `logout()` stops polling first.
- `frontend/todo-app/src/app/features/tasks/task.service.ts` (Task 13) — create/update/complete/delete refresh the bell (task step "Also refresh after any task change").
- `frontend/todo-app/src/app/features/dashboard/dashboard-page/dashboard-page.component.ts` (Task 10 placeholder) — replaced with the real page.
- `plans/00-INDEX.md` — Task 15 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-12 | ✅ PASS |
