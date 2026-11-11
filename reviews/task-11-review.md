# Review — Task 11: Frontend Login & Register

**Date:** 2026-11-11 · **Branch:** task/11-frontend-auth · **Verdict:** ✅ PASS

## Summary
This task adds the Register and Login pages: Material reactive forms with inline validation that mirrors the API rules, server-error mapping, and a disabled button while a request is in flight. Login has a safe `returnUrl` and specific messages for 401 / 423 / 429. Every check was done end-to-end in the browser with keyboard input against the real API: registration with default categories, duplicate email, logout and reload, wrong password, lockout after 5 attempts, rate limit, returnUrl, and a new tab without a session. Two small fixes came out of the testing and are included: focusing the first invalid field after server errors, and Prettier on CRLF checkouts.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | ✅ All files pass linting |
| Prettier | `npm run format:check` | ✅ clean, including on CRLF working copies (`endOfLine: auto`) |
| Strict mode | `tsconfig.json` | ✅ `strict`, `strictTemplates` |
| Build | `npx ng build` | ✅ no warnings; initial 472 kB (< 500 kB budget) |
| `innerHTML` / `outerHTML` / `bypassSecurityTrust` | grep changed files | ✅ none |
| `console.*`, TODO, FIXME | grep changed files | ✅ none |
| Password/token storage | grep `localStorage`/`sessionStorage` in changed files | ✅ none (only `AuthService` from Task 10 stores the session; passwords are never stored) |
| `userId` sent | grep | ✅ none |
| Packages | `git diff main...HEAD -- package*.json` | ✅ unchanged |
| One public type per file | count | ✅ |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/11-frontend-auth` from `main` (f7055e3); 3 commits, all `[Task 11] ...` |
| Tracked junk / remote | `git ls-files`, `git remote -v` | ✅ none |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| New user can register and lands on the dashboard; Work and Personal exist (via API) | ✅ | Keyboard-only register → `/dashboard`, header "Task Eleven"; `GET /api/v1/categories` → `Personal`, `Work` |
| Logout → refresh → still logged out; login works again | ✅ | Logout (Enter on the focused button) → `/login`, session `null`; reload `/categories` → `/login?returnUrl=%2Fcategories`; login → back on `/categories` |
| Wrong password 5 times shows the lockout message | ✅ | Attempts 1–4 "Invalid email or password"; attempt 5 and the 6th with the correct password: "Account locked after too many attempts. Try again in 15 minutes." |
| Closing the tab and reopening requires login (sessionStorage) | ✅ | New tab (no session copied) → `/login?returnUrl=%2Fdashboard` while the other tab stayed logged in |
| Forms fully usable with the keyboard only | ✅ | Typing, Tab between fields, Enter to submit, Enter on Logout; focus moves to the first invalid field (Name / Password / Email depending on the case) |

Steps check:
- Register: Name (required, max 100), Email (required, email, max 256 like the API), Password (required, min 8, letter + digit), Confirm (must match). Inline errors after touch/submit; button disabled while submitting; hint "Min 8 characters, with a letter and a number"; success → session + `/dashboard`; `400` → "An account with this email already exists." under Email; link to Login. ✅
- Login: Email, Password; success → returnUrl or `/dashboard`; 401 / 423 / 429 messages exactly as specified; link to Register. ✅
- Logout: clears sessionStorage and state, goes to `/login`. ✅
- Accessibility: `<label for>` on every input, errors in `aria-describedby` (checked on Name: `mat-mdc-error-0` = "Name is required."), Enter submits, focus to the first invalid field, form messages in `role="alert"`. ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–10 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blockers |
| G-SCOPE-1, 2 | ✅ | Matches the task file. The returnUrl allow-list is a safety measure for the step "go to the originally requested URL", not a feature |
| G-SCOPE-3 | ✅ | No "forgot password" or other Phase 2 items |
| G-SCOPE-4 | ✅ | `error.interceptor.ts` (Task 10): no 429 toast for the login call, to avoid a double message. `.prettierrc` (Task 10): `endOfLine: auto` so Prettier passes on Windows CRLF checkouts |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1 | ✅ | No secrets; test passwords exist only in the browser session of the manual test |
| G-SEC-2 | ✅ | No logging |
| G-SEC-3 | ✅ | No user id sent |
| G-SEC-4 | N/A | — |
| G-SEC-5 | ✅ | Interpolation only, including server messages (`{{ }}`) |
| G-SEC-6 to 8 | N/A | No backend change |
| G-CODE-1, 2 | ✅ | Strict; lint + Prettier clean |
| G-CODE-5 | ✅ | kebab-case files, `*.component.ts` / `*.validator.ts` |
| G-CODE-6 | ✅ | One exported type per file (plus related constants) |
| G-CODE-7 | ✅ | No dead code; `shared/.gitkeep` removed now that `shared/` has files |
| G-CODE-8 | ✅ | Limits in `AUTH_FORM_RULES`. "15 minutes" appears only as message text required by the task (NFR-11) |
| G-TEST-1 to 4 | N/A | No frontend tests in v1 (Phase 2); backend unchanged |
| G-DB-1 to 4 | N/A | — |
| G-DESTRUCT-1 to 4 | ✅ | Leftover dev-server processes started by me in this session were stopped; no other deletions |

## Issues to fix (FAIL only)
None.

## Not verified
- **Screen reader:** ARIA wiring was checked in the DOM (`aria-describedby`, `label for`, `role="alert"`), not with an actual screen reader (NVDA/Narrator). Full accessibility pass is Task 16.
- **Bearer header on protected calls:** `authInterceptor` is not used by these pages (login/register are anonymous). It gets exercised by the category pages in Task 12.
- **Test data in `TodoAppDb`:** users `task11-ui@example.test` (now locked for 15 minutes) and `task11-tab-…@example.test`.

## Files changed outside this task's scope
- `frontend/todo-app/src/app/core/http/error.interceptor.ts` (Task 10) — no 429 toast for the login call; the login page shows it inline.
- `frontend/todo-app/.prettierrc` (Task 10) — `"endOfLine": "auto"` (with `core.autocrlf=true`, Windows checkouts would otherwise fail `prettier --check`).
- `plans/00-INDEX.md` — Task 11 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
