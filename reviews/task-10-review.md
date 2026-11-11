# Review — Task 10: Angular Scaffold & Core Setup

**Date:** 2026-11-11 · **Branch:** task/10-frontend-scaffold · **Verdict:** ✅ PASS

## Summary
The Angular 22 app `frontend/todo-app` is scaffolded with standalone components, routing, SCSS and **Angular Material** (chosen by the client). ESLint and Prettier are set up, and strict TypeScript and templates are on. The core layer has the API models, an `AuthService` that stores the session in `sessionStorage` and logs out automatically on expiry, an auth interceptor and an error interceptor (401 / 429 / 0+5xx handling, ProblemDetails read into `ApiError`), auth and guest guards, and a snackbar toast service. A Material shell holds lazy placeholder routes. Lint, Prettier and the build are clean. In the browser, the redirect, proxy, guards, auto-logout and logout all worked with no console errors.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | ✅ All files pass linting |
| Prettier | `npx prettier --check "src/**/*.{ts,html,scss}"` (`npm run format:check`) | ✅ All matched files use Prettier code style |
| Strict mode | `tsconfig.json` | ✅ `"strict": true`, `"strictTemplates": true` (plus `strictInjectionParameters`, `strictInputAccessModifiers`) |
| Build | `npx ng build` | ✅ succeeds with no warnings; initial 456 kB (< 500 kB warning budget); every page lazy |
| Backend unchanged | `git diff main...HEAD -- backend` | ✅ no backend changes |
| `innerHTML` / `outerHTML` / `bypassSecurityTrust` | grep `src` | ✅ none |
| `console.*`, `TODO`, `FIXME`, `fdescribe`, `xit(`, `fit(` | grep `src` | ✅ only `console.error` in the generated bootstrap `catch` in `main.ts` (allowed by the ESLint `no-console` rule; not `console.log`) |
| Secrets / `userId` sent | grep `src` | ✅ none; the app never sends a user id |
| One public type per file / kebab-case files | per-file count, path check | ✅ |
| Packages | `npm ls --depth=0` | ✅ only `ng new` packages, `@angular/material` + `@angular/cdk` (Material chosen), `angular-eslint` (with its `eslint`, `@eslint/js`, `typescript-eslint`), `prettier`, `eslint-config-prettier`, all on Appendix A |
| Vulnerabilities | `npm audit --omit=dev` / `npm audit` | ✅ production: 0. ⚠️ dev-only: moderate `minimatch` advisory through the ESLint toolchain (see Not verified) |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/10-frontend-scaffold` from `main` (ec9dd2c); 3 commits, all `[Task 10] ...` |
| Tracked junk | `git ls-files` | ✅ no `node_modules/`, `dist/`, `.angular/` |
| Remote | `git remote -v` | ✅ none |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| `ng serve` runs; `http://localhost:4200` loads without console errors | ✅ | Browser pane: only Vite/"development mode" messages; `onlyErrors` → none |
| `/tasks` while logged out redirects to `/login` | ✅ | → `/login?returnUrl=%2Ftasks` |
| An API call from the app reaches the backend through the proxy (no CORS error) | ✅ | `fetch('/api/v1/auth/register')` from the app page → `201`; `GET http://localhost:4200/api/v1/health` → `200`, `/categories` → `401` (from the backend) |
| `ng build` succeeds | ✅ | see automated checks |

Steps check:
- `ng new` (standalone, routing, SCSS), folder structure `core/`, `shared/`, `features/{auth,tasks,categories,dashboard,notifications}`. ✅ (`shared/` and `features/notifications/` hold `.gitkeep` until used)
- `environment.ts` / `environment.development.ts` with `apiBaseUrl: '/api/v1'`; `proxy.conf.json` to `https://localhost:7146`, `secure: false`, wired in `angular.json`. ✅
- UI kit: Angular Material; base styles (Roboto, spacing tokens, focus outline, AA-contrast toast colors). ✅
- Models for every listed DTO. ✅
- `AuthService` (login/register/logout, `token`/`currentUser` signals, `isLoggedIn()` checks `expiresAt`, sessionStorage). ✅ Logout and auto-logout verified in the browser.
- `authInterceptor`, `errorInterceptor`. ✅ in code. ⚠️ Not exercised at runtime yet: no page makes an `HttpClient` call until Task 11 (see Not verified).
- `authGuard` / `guestGuard`. ✅ both verified in the browser.
- `ToastService`. ✅ (session-expired toast seen)
- Auto-logout timer. ✅ (`expiresAt` = now + 3 s → logged out with toast)
- Shell (app name, nav, bell placeholder, user name, Logout) and routes (`/login`, `/register` guest; `/dashboard`, `/tasks`, `/tasks/:id`, `/categories` in shell with auth; `''` and `**` → dashboard). ✅ (`/tasks/42` shows "Task 42")
- Placeholder components. ✅

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–09 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | UI kit decision asked; client chose Angular Material |
| G-SCOPE-1, 2 | ✅ | Matches the task file and ARCHITECTURE §7, §10. The "Skip to content" link and focus outline serve the ARCHITECTURE §10 accessibility rules. ARCHITECTURE §10 updated to record the UI kit choice (no scope change) |
| G-SCOPE-3 | ✅ | No frontend tests (Phase 2); `tsconfig.spec.json` / `test` script removed because there is no test target |
| G-SCOPE-4 | ✅ | Only `ARCHITECTURE.md` (UI kit row) and the index outside `frontend/` |
| G-GIT-1 to 5 | ✅ | See automated checks; `.gitignore` covers `node_modules/`, `dist/`, `.angular/` |
| G-DEP-1 | ✅ | All on Appendix A (Material allowed because it was chosen in Task 10) |
| G-DEP-2 | N/A | No new package types |
| G-DEP-3 | ✅ | Stable releases, MIT licences |
| G-SEC-1 | ✅ | No secrets in the frontend |
| G-SEC-2 | ✅ | No logging of tokens |
| G-SEC-3 | ✅ | No user id sent from the client |
| G-SEC-4 | N/A | No SQL |
| G-SEC-5 | ✅ | No `innerHTML`/`bypassSecurityTrust`; interpolation only |
| G-SEC-6 to 8 | N/A | No backend change |
| G-CODE-1 | ✅ | `strict`, `strictTemplates`; build has no warnings |
| G-CODE-2 | ✅ | `ng lint` + Prettier clean |
| G-CODE-3, 4 | N/A | Backend rules |
| G-CODE-5 | ✅ | kebab-case files (`*.component.ts`, `*.service.ts`, `*.guard.ts`, `*.interceptor.ts`) |
| G-CODE-6 | ✅ | One exported type per model file |
| G-CODE-7 | ✅ | No `console.log`, TODO or dead code |
| G-CODE-8 | ✅ | Storage key, toast durations and messages are named constants; token expiry comes from the API |
| G-TEST-1 to 4 | N/A | No frontend tests in v1; backend unchanged (137 tests) |
| G-DB-1 to 4 | N/A | — |
| G-DESTRUCT-1 to 4 | ✅ | Only files created in this task were removed (generated `app.html`/`app.scss`, `tsconfig.spec.json`) |

## Issues to fix (FAIL only)
None.

## Not verified
- **Interceptors at runtime:** `authInterceptor` (Bearer header) and `errorInterceptor` (401/429/5xx + `ApiError`) compile and pass lint, but no page calls `HttpClient` yet. They are exercised by the login and register forms in Task 11.
- **Dev-only `npm audit` finding:** moderate `minimatch` ReDoS (GHSA-g3hv-75wg-m249) through `eslint` / `typescript-eslint` (lint tooling only, not in the app bundle). The suggested fix downgrades ESLint (`npm audit fix --force` → `eslint@9`), which needs your OK (G-DESTRUCT-4). I recommend waiting for a patched `typescript-eslint`.
- **Visual design:** checked by screenshot only. Contrast and keyboard checks are part of Task 16.
- **Verification data in `TodoAppDb`:** users `task10-…@example.test`, created through the proxy.

## Files changed outside this task's scope
- `ARCHITECTURE.md` — §10 "UI kit" row records the client's choice (Angular Material).
- `plans/00-INDEX.md` — Task 10 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
