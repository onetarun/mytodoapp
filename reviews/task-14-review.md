# Review — Task 14: Frontend: SubTasks / Checklist

**Date:** 2026-11-12 · **Branch:** task/14-frontend-subtasks · **Verdict:** ✅ PASS

## Summary
The task detail page now has a full checklist: "x of y done" text with a progress bar, tick/untick, inline rename (Enter saves, Esc cancels), delete with an "Item deleted" toast (no dialog) and an "Add item" field that keeps focus for fast entry. `SubTaskService` calls the four Task 07 endpoints. Everything was tested in the browser against the real API using only the keyboard, and the soft delete was confirmed in SQL Server. No backend code changed.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | ✅ "All files pass linting." |
| Prettier | `npx prettier --check "src/**/*.{ts,html,scss}"` | ✅ "All matched files use Prettier code style!" |
| Strict mode | `tsconfig.json` | ✅ `"strict": true`, `"strictTemplates": true` |
| Build | `npx ng build` | ✅ complete, no warnings; initial 502.66 kB (under the approved 650 kB budget) |
| Backend build/format/tests/coverage | — | N/A: `git diff main...HEAD` has no `backend/` changes |
| Code searches (changed files) | Grep: `innerHTML`, `outerHTML`, `bypassSecurityTrust`, `console.log`, `TODO`, `FIXME`, `xit(`/`fit(`/`fdescribe`/`xdescribe`, `token`, `localStorage`, `//` comment lines | ✅ no matches |
| Packages | `git diff main...HEAD -- package.json package-lock.json` | ✅ no changes |
| Tracked build output | `git ls-files` for `node_modules/`, `bin/`, `obj/`, `logs/`, `.bak` | ✅ none |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Step: `SubTaskService` `create`/`update`/`setCompletion`/`delete` | ✅ | `features/tasks/sub-task.service.ts`; URLs match ARCHITECTURE §API (POST, PUT, PATCH `/complete`, DELETE under `/tasks/{id}/subtasks`) |
| Step: progress text and bar | ✅ | `task-checklist.component.html`; browser "2 of 3 done", bar `aria-valuenow` 67 |
| Step: checkbox, title, inline Edit, Delete per item | ✅ | Template + browser test |
| Step: "Add item" at the bottom, Enter adds and keeps focus | ✅ | 3 items added by typing + Enter; `document.activeElement` was still the Add item input |
| Step: items in `sortOrder` | ✅ | `sortBySortOrder()`; `sqlcmd` SortOrder 1-2-3 matches the shown order after reload |
| Step: delete without dialog + "Item deleted" toast | ✅ | Toast text "Item deleted" seen in the browser |
| Step: list "x/y" up to date after going back | ✅ | List page loads fresh data; after ticking → "Checklist 2/3", after deleting → "Checklist 1/2" |
| DoD: add 3, tick 2 → "2 of 3 done" and list "2/3" | ✅ | Browser, real API (task 2012) |
| DoD: edit and delete work; deleted items don't count | ✅ | Rename saved (toast "Item updated."), Esc discarded a second edit; deleting the ticked item → "1 of 2 done" / list "1/2"; `sqlcmd`: `SubTasks.Id 9 IsDeleted = 1` |
| DoD: fully usable with the keyboard | ✅ | Whole flow with Tab/Shift+Tab, Space, Enter, Esc; focus returns to the item's Edit button after save/cancel and to "Add item" after delete; empty Enter shows "Item text is required." |

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–13 ✅ in the index |
| G-PROC-2 | ✅ | Client ran `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 to 7 | ✅ | No blockers; the branch being checked out in another worktree was resolved with the client's choice |
| G-SCOPE-1, 2, 3 | ✅ | Only FR-11 to FR-13 / task file items; no Phase 2 work. The "Item updated." toast is small feedback in line with the other screens |
| G-SCOPE-4 | ✅ | Task 13 detail page files changed only to host the checklist (listed below) |
| G-GIT-1 | ✅ | `task/14-frontend-subtasks`, from `main` at `1544173` |
| G-GIT-2 | ✅ | All 3 commits start with `[Task 14]` |
| G-GIT-3 | ✅ | Not merged; waiting for client approval |
| G-GIT-4 | ✅ | No remote configured, no upstream |
| G-GIT-5 | ✅ | `.claude/launch.json` (local dev-server config for testing) is untracked and not committed |
| G-DEP-1 to 3 | ✅ | No package changes |
| G-SEC-1, 2 | ✅ | No secrets or logging in the diff |
| G-SEC-3 | ✅ | Requests send only `title` / `isCompleted`; no `UserId` |
| G-SEC-4 | N/A | No backend changes |
| G-SEC-5 | ✅ | Item titles shown only with `{{ }}`; no `innerHTML` |
| G-SEC-6 to 8 | N/A | Backend unchanged (verified in Task 07); a `404` gives a toast in the UI |
| G-CODE-1, 2 | ✅ | Build, lint, Prettier clean; strict on |
| G-CODE-3, 4 | N/A | Backend only |
| G-CODE-5, 6 | ✅ | kebab-case files, one class per file |
| G-CODE-7 | ✅ | No dead code, `console.log` or TODO |
| G-CODE-8 | ✅ | Title limit from `TASK_RULES.titleMaxLength` (200, same as `SubTask.TitleMaxLength`); `PERCENT` is a named constant, not a business rule |
| G-TEST-1 to 4 | N/A | No frontend tests in v1 (same as Tasks 10–13); backend unchanged |
| G-DB-1, 2, 4 | N/A | No schema or date changes |
| G-DB-3 | ✅ | Delete uses the soft-delete endpoint; `IsDeleted = 1` confirmed |
| G-DESTRUCT-1 to 4 | ✅ | Nothing destructive. The other worktree (`start-process-c63666`, clean) was switched to a detached HEAD with the client's approval so this branch could be checked out here; no work lost |

## Issues to fix (FAIL only)
None.

## Not verified
- **Screen reader:** `aria-label`s ("Mark as done: …", "Edit …", "Delete …", progress bar label) and `aria-live` on the progress text were checked in the DOM, not with NVDA/Narrator (Task 16).
- **Mobile widths (360 px):** wrapping flex layout, not checked at 360 px (Task 16).
- **Subtask title limit:** the UI reuses the task title limit (`TASK_RULES.titleMaxLength`, 200). It matches the API today; if `SubTask.TitleMaxLength` changes later, the UI must change too.
- **Test data in `TodoAppDb`:** user `task14-checklist@example.test`, task "Plan trip" (Id 2012) with 3 subtasks (1 soft-deleted).

## Files changed outside this task's scope
- `frontend/todo-app/src/app/features/tasks/task-detail-page/task-detail-page.component.{html,scss,ts}` (Task 13) — the "x/y done" placeholder section was replaced by `<app-task-checklist>`, and its styles moved to the new component.
- `plans/00-INDEX.md` — Task 14 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-12 | ✅ PASS |
