# Review — Task 13: Frontend Tasks

**Date:** 2026-11-11 · **Branch:** task/13-frontend-tasks · **Verdict:** ✅ PASS

## Summary
The main task screen is built: quick add (title + Enter), "Add details" with description, local due date/time, priority and category, an All / Pending / Completed filter kept in `?status=`, and task rows. Each row has a checkbox, a priority text badge, the category or "Uncategorized", the local due date, checklist progress, and an overdue marker (red + icon + "Overdue"). An edit dialog, delete with confirmation and a detail page with a "Task not found" state are included. Everything was tested in the browser against the real API, and the UTC storage of due dates was confirmed in SQL Server. Two decisions came up during the task: the client approved raising the initial bundle budget to 650 kB, and the "Uncategorized" placeholder fix was found while testing.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | ✅ All files pass linting |
| Prettier | `npm run format:check` | ✅ clean |
| Strict mode | `tsconfig.json` | ✅ unchanged (`strict`, `strictTemplates`) |
| Build | `npx ng build` | ✅ no warnings under the approved budget; initial 502.66 kB raw / 124.6 kB transfer; task pages lazy |
| `innerHTML` / `outerHTML` / `bypassSecurityTrust` | grep changed files | ✅ none; titles and descriptions only through interpolation (description uses `white-space: pre-wrap`, not HTML) |
| `console.*`, TODO, FIXME | grep | ✅ none |
| `userId` sent | grep | ✅ none |
| Packages | `git diff main...HEAD -- package*.json` | ✅ unchanged |
| One public type per file | count | ✅ |
| Every `<form>` bound to a FormGroup | grep templates | ✅ 5/5 |
| Magic numbers | grep | ✅ only `TASK_RULES.titleMaxLength` / `descriptionMaxLength` (named, mirror the API) |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/13-frontend-tasks` from `main` (c6923dd); 3 commits, all `[Task 13] ...` |
| Tracked junk / remote | `git ls-files`, `git remote -v` | ✅ none (`dist/` stats file deleted, `dist/` ignored) |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Typing a title and pressing Enter creates a task | ✅ | "Buy groceries" + Enter → in the list, toast "Task added.", field cleared and focused |
| Filter All/Pending/Completed shows the right tasks and survives completing a task and a refresh | ✅ | Pending → `?status=pending`; completing "Pay electricity bill" (Space on its checkbox) removes it from Pending; refresh keeps Pending selected; Completed shows it |
| Past-due pending task shows red + icon + "Overdue"; completing it removes the marker | ✅ | Red left border and background, ⚠ icon, "Overdue" text; after completion: no `overdue` class, no label, line-through |
| Edit and delete (with confirmation) work | ✅ | Edit dialog prefilled → title changed → saved; delete dialog: Esc keeps the task, Delete removes it ("Task deleted.") |
| Due dates display in local time and are saved correctly (UTC in SQL) | ✅ | Input 15-12-2026 09:30 in UTC+5:30 → API `2026-12-15T04:00:00Z`, list "Dec 15, 2026, 9:30:00 AM"; `sqlcmd`: `DueDate 2026-12-15 04:00:00.0000000` (also after editing) |

Steps check:
- `TaskService`: `getAll(status)`, `getById`, `create`, `update`, `setCompletion`, `delete`. ✅
- Quick add ≤ 2 actions (type + Enter, NFR-21); "Add details" toggle (`aria-expanded`) reveals all fields. ✅
- Status filter default All, kept in the URL, stays applied after add/edit/complete/delete (the list reloads with the active filter). ✅
- Row shows checkbox, title (link to `/tasks/:id`), priority as text, category or "Uncategorized", local due date, `x/y` progress (seen "1/2"). ✅
- Checkbox toggles immediately with a toast; completed shows line-through. ✅
- Overdue red + icon + text (FR-21, NFR-24). ✅
- Row Edit and Delete icon buttons with `aria-label`s; delete confirmation (FR-8). ✅
- Empty states: "No tasks yet — add your first one", "No pending tasks 🎉", "No completed tasks yet". ✅
- Loading spinner (`aria-label="Loading tasks"`). ✅
- Form: Title required/max 200, Description max 2000 (with counter), `datetime-local` → UTC ISO, Priority default Medium, Category from `CategoryService` including "Uncategorized"; server errors mapped to fields. ✅
- Detail page: all fields, Edit button, checklist placeholder ("1/2 done"); `404` and invalid id → "Task not found" + link. ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–12 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | The bundle budget warning was a decision for the client; asked and answered (650 kB) |
| G-SCOPE-1, 2 | ✅ | Matches the task file. The detail page shows only the subtask counts; the checklist UI is Task 14 |
| G-SCOPE-3 | ✅ | No search, sorting options, bulk actions or other Phase 2 items |
| G-SCOPE-4 | ✅ | `app-shell` (Task 10): tooltip removed from the disabled bell (it never showed; saved ~18 kB). `angular.json` budget and `GUARDRAILS.md` change log: client-approved. `styles.scss`: shared `.visually-hidden` helper |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added (button-toggle, checkbox, select are part of `@angular/material`) |
| G-SEC-1 to 3 | ✅ | No secrets, no logging, no user id sent |
| G-SEC-4 | N/A | — |
| G-SEC-5 | ✅ | Interpolation only |
| G-SEC-6 to 8 | N/A | Backend unchanged; another user's task id → API `404` → "Task not found" |
| G-CODE-1 | ✅ | Strict; no build warnings with the approved budget |
| G-CODE-2 | ✅ | Lint + Prettier clean |
| G-CODE-5, 6, 7 | ✅ | kebab-case, one type per file, no dead code |
| G-CODE-8 | ✅ | `TASK_RULES`; HTTP codes through `HttpStatusCode` |
| G-TEST-1 to 4 | N/A | No frontend tests in v1 (Phase 2); backend unchanged |
| G-DB-1 to 3 | N/A | — |
| G-DB-4 | ✅ | Client sends UTC ISO; SQL shows UTC |
| G-DESTRUCT-1 to 4 | ✅ | Only this session's leftover dev-server processes were stopped |

## Issues to fix (FAIL only)
None.

## Not verified
- **Screen reader:** labels (`aria-label` on the checkbox and icon buttons, "High priority", "Due …", "Checklist …") checked in the DOM, not with NVDA/Narrator (Task 16).
- **Mobile widths (360 px):** the layout uses wrapping flex/grid, but it was not checked at 360 px. Responsive polish is Task 16.
- **Concurrent edits** (the same task changed in two tabs): a `404` gives a toast and reloads the list; there is no conflict detection (not required in v1).
- **Test data in `TodoAppDb`:** users `task13-…@example.test` (3 tasks, 2 subtasks) and `task13-empty-…@example.test`.

## Files changed outside this task's scope
- `frontend/todo-app/angular.json` — initial budget warning 500 kB → 650 kB (client decision, 2026-11-11).
- `GUARDRAILS.md` — Appendix B change log entry for the budget decision.
- `frontend/todo-app/src/app/core/layout/app-shell/app-shell.component.{html,ts}` (Task 10) — removed the tooltip from the disabled bell button.
- `frontend/todo-app/src/styles.scss` — `.visually-hidden` utility for screen-reader-only text.
- `plans/00-INDEX.md` — Task 13 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
