# Review — Task 12: Frontend Categories

**Date:** 2026-11-11 · **Branch:** task/12-frontend-categories · **Verdict:** ✅ PASS

## Summary
This task adds `CategoryService`, a cached `categories` signal for Task 13, and the `/categories` page: a list with task counts, inline add (Enter), inline rename (Enter saves, Esc cancels), delete with the FR-16 confirmation text, a not-blank / max 50 check, the server's duplicate-name error, an empty state, and toasts for every action. A reusable accessible confirm dialog is added under `shared/`. Everything was tested in the browser with the keyboard against the real API. Testing found one bug: Enter reloaded the page because the forms had no `[formGroup]`. It was fixed on this branch.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | ✅ All files pass linting |
| Prettier | `npm run format:check` | ✅ clean |
| Build | `npx ng build` | ✅ no warnings; initial 481 kB (< 500 kB); categories page is a lazy chunk |
| `innerHTML` / `outerHTML` / `bypassSecurityTrust` | grep changed files | ✅ none; names shown only through interpolation (G-SEC-5) |
| `console.*`, TODO, FIXME | grep | ✅ none |
| `userId` sent to the API | grep | ✅ never sent. `category.service.ts:33–35` only reads the current user id to clear the client cache when the user changes |
| Packages | `git diff main...HEAD -- package*.json` | ✅ unchanged |
| One public type per file | count | ✅ (`ConfirmDialogData` in its own file) |
| Every `<form>` bound to a FormGroup | grep templates | ✅ 4/4 |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/12-frontend-categories` from `main` (0c26bd5); 4 commits, all `[Task 12] ...` |
| Tracked junk / remote | `git ls-files`, `git remote -v` | ✅ none |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| A new user sees Work and Personal | ✅ | New user → `/categories` shows Personal (0 tasks), Work (1 task) |
| Add, rename and delete work and the list updates without a page reload | ✅ | Add "Home" (Enter) → in the list, field cleared; rename Home → "Family" (Enter); delete Work (dialog → Delete). Each with a success toast and no reload |
| After deleting a category, its tasks still exist (via API) with no category | ✅ | `GET /api/v1/tasks` → "Quarterly report", `categoryId: null`, `categoryName: null` |
| Duplicate names show a clear error | ✅ | "work" → "You already have a category with this name." under the field and as a toast |

Steps check:
- `CategoryService`: `getAll`, `create`, `update`, `delete`, cached `categories` signal that refreshes after each change (+ `ensureLoaded`). ✅
- List with name and task count; inline add with Enter; inline rename with Save/Cancel and Esc; delete dialog with the exact FR-16 text ("Delete 'Work'? Its 1 task will become Uncategorized."); required (blank → "Name is required.") and max 50; server duplicate error; empty state "No categories yet". ✅
- Success and error toasts for every action. ✅

Keyboard and accessibility (NFR-23) also verified: Rename/Delete icon buttons have `aria-label`s ("Rename Home", "Delete Work"). The dialog is `role="alertdialog"` with initial focus on Cancel, Esc closes it, and focus returns to the trigger. After rename and cancel, focus goes back to the row's Rename button; after add and delete, to the "New category" field.

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–11 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1, 2 | ✅ | Matches the task file. The confirm dialog is the shared building block that FR-8 (Task 13) also needs |
| G-SCOPE-3 | ✅ | No colors/icons per category, reordering or other extras |
| G-SCOPE-4 | ✅ | Only this task's files (plus the index) |
| G-GIT-1, 3, 4, 5 | ✅ | See automated checks |
| G-GIT-2 | ✅ | Format OK. Commit `6fca277`'s message also says "tick steps and record verification", but that commit holds only the form fix (the plan update went to `ebc22fd`). Rewording would rewrite history (G-DESTRUCT-2), so it was left as is |
| G-DEP-1 to 3 | ✅ | No packages added (`MatDialog`, `MatProgressSpinner` are part of `@angular/material`) |
| G-SEC-1 to 3 | ✅ | No secrets, no logging, no user id sent |
| G-SEC-4 | N/A | — |
| G-SEC-5 | ✅ | Interpolation only |
| G-SEC-6 to 8 | N/A | No backend change (ownership enforced by the API, Task 05) |
| G-CODE-1, 2 | ✅ | Strict; lint + Prettier clean |
| G-CODE-5, 6, 7 | ✅ | kebab-case, one type per file, no dead code |
| G-CODE-8 | ✅ | `CATEGORY_NAME_MAX_LENGTH`; HTTP codes through `HttpStatusCode` |
| G-TEST-1 to 4 | N/A | No frontend tests in v1 (Phase 2); backend unchanged |
| G-DB-1 to 4 | N/A | — |
| G-DESTRUCT-1 to 4 | ✅ | Only leftover dev-server processes of this session were stopped |

## Issues to fix (FAIL only)
None.

## Not verified
- **Screen reader announcement** of toasts and the dialog: wiring checked (`politeness`, `alertdialog`), not listened to with NVDA/Narrator (Task 16).
- **Max-length error message:** `maxlength="50"` on the input stops typing past 50, so the "50 characters or fewer" message is only a fallback. The API limit is enforced either way.
- **Bundle budget:** initial bundle is at 481 kB of the 500 kB warning budget. Later pages are lazy, but this will be watched in Tasks 13–16.
- **Test data in `TodoAppDb`:** user `task12-…@example.test` with one task ("Quarterly report") and soft-deleted categories.

## Files changed outside this task's scope
- `plans/00-INDEX.md` — Task 12 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
