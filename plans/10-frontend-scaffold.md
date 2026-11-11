# Task 10 — Angular Scaffold & Core Setup

**Depends on:** 09
**References:** FR-5, NFR-10, NFR-18, NFR-19 · ARCHITECTURE §7 (client storage), §10
**Goal:** An Angular app that talks to the API, attaches the JWT automatically, protects routes and handles expired sessions and errors.

## Steps

### Project
- [x] `ng new todo-app` inside `frontend/` (standalone components, routing, SCSS).
- [x] Folder structure: `src/app/core/`, `src/app/shared/`, `src/app/features/{auth,tasks,categories,dashboard,notifications}/`.
- [x] `environment.ts` / `environment.development.ts` with `apiBaseUrl` (e.g. `/api/v1`).
- [x] `proxy.conf.json` forwarding `/api` to the HTTPS API (`secure: false` for the dev cert); wire it in `angular.json` `serve` options.
- [x] Choose the UI kit (Angular Material **or** plain SCSS) and set base styles: font, spacing, colors with good contrast.

### Core (`core/`)
- [x] `models/` — TypeScript interfaces mirroring the API DTOs (AuthResponse, TaskListItem, TaskDetail, SubTask, Category, DashboardSummary, Notification, Priority, TaskStatusFilter).
- [x] `AuthService` — `login()`, `register()`, `logout()`, `token` and `currentUser` (signal), `isLoggedIn()` that also checks `expiresAt`. Store `{ token, expiresAt, user }` in **`sessionStorage`**.
- [x] `authInterceptor` (functional) — adds `Authorization: Bearer <token>` to API calls.
- [x] `errorInterceptor` — on `401`: logout + redirect to `/login`; on `429`: "Too many requests, try again shortly"; on `0`/`5xx`: generic friendly message. Read `title`/`errors` from ProblemDetails for `400`.
- [x] `authGuard` (functional) — redirect to `/login` when not logged in; `guestGuard` — redirect logged-in users away from login/register.
- [x] `ToastService` / snackbar for success and error messages.
- [x] Auto-logout timer: when `expiresAt` passes, log out and redirect (NFR-10).

### Shell & routing
- [x] `AppShell` layout: header (app name, nav links: Dashboard, Tasks, Categories; bell placeholder; user name; Logout) and main content area.
- [x] Routes (lazy-loaded feature routes): `/login`, `/register` (guestGuard); `/dashboard`, `/tasks`, `/tasks/:id`, `/categories` (authGuard, inside the shell); `''` → `/dashboard`; `**` → `/dashboard`.
- [x] Placeholder components for each route.

## Definition of Done

- [x] `ng serve` runs; `http://localhost:4200` loads without console errors.
- [x] Opening `/tasks` while logged out redirects to `/login`.
- [x] An API call from the app reaches the backend through the proxy (no CORS error).
- [x] `ng build` succeeds.

## Notes

- **UI kit:** the client chose **Angular Material** (2026-11-11), added with `ng add @angular/material` (Material 3 azure/blue theme, Roboto, Material Symbols). ARCHITECTURE §10 updated.
- **Scaffold:** `ng new todo-app --directory=frontend/todo-app --routing --style=scss --ssr=false --skip-git --skip-tests --ai-config=none`. Angular CLI 22.0.7, zoneless by default.
  - `--skip-tests`: frontend tests are Phase 2 (ARCHITECTURE §11), so `tsconfig.spec.json` and the `test` script were removed (there is no test target).
- **Strictness (G-CODE-1):** `"strict": true` and `"strictTemplates": true` are set explicitly in `tsconfig.json`.
- **Lint/format (G-CODE-2):** `ng add angular-eslint` (also adds `eslint`, `@eslint/js`, `typescript-eslint`), `eslint-config-prettier` as the last config entry, and `no-console` (only `console.error` allowed, for the bootstrap failure in `main.ts`). Scripts: `npm run lint`, `npm run format:check`.
- **Proxy:** `proxy.conf.json` forwards `/api` to `https://localhost:7146` (`secure: false` for the dev cert), wired in `angular.json` `serve.options.proxyConfig`. The API therefore needs the `https` launch profile.
- **Models:** one type per file in `core/models/` (G-CODE-6). `TaskNotification` is named so it does not clash with the DOM `Notification` type. Request DTOs for tasks and categories are added with their feature tasks.
- **Errors:** `errorInterceptor` re-throws every HTTP error as `core/http/ApiError`, which holds `status`, the ProblemDetails `title` as `message`, `fieldErrors` and `fieldError(name)`, so pages can show 400/404/423 details.
  - 401 on any call except `/auth/login` → toast + logout.
  - 429 → "Too many requests, try again shortly."
  - 0 / 5xx → friendly message.
- **Auth state:** `AuthService` keeps the `AuthResponse` in `sessionStorage['todo-app.session']` and checks it at startup (invalid or expired → cleared). It exposes `token` / `currentUser` signals and `isLoggedIn()`, and sets a timer that logs out with a toast when `expiresAt` passes.
- **Routing:** `authGuard` adds `?returnUrl=` (used by the login page in Task 11). Feature routes are lazy (`loadChildren`), and the task detail page gets `:id` through `withComponentInputBinding()`.
- **Shell:** a "Skip to content" link, `aria-current` on the active nav link, and a disabled bell placeholder (wired in Task 15). Global styles define spacing tokens, a visible `:focus-visible` outline and AA-contrast toast colors.

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | All files pass linting |
| Prettier | `npm run format:check` | All matched files use Prettier code style |
| Build | `npx ng build` | succeeds, no warnings; initial 456 kB (budget warning at 500 kB), each page lazy |
| `ng serve` + console | `ng serve`, open `http://localhost:4200` in the browser pane | loads; console has only Vite/Angular dev messages, no errors |
| Logged-out redirect | open `/tasks` | → `/login?returnUrl=%2Ftasks` |
| Proxy | `curl http://localhost:4200/api/v1/health` / `/categories`; `fetch('/api/v1/auth/register')` from the app page | `200` / `401` from the backend; register `201` through the proxy, no CORS error |
| Logged-in shell | session stored, open `/dashboard` | header with Dashboard/Tasks/Categories, bell, user name, Logout; `/tasks/42` shows "Task 42" |
| Guest guard | open `/login` while logged in | → `/dashboard` |
| Auto-logout | `expiresAt` set to now + 3 s | after expiry → `/login`, session removed, toast "Your session has expired…" |
| Logout | click Logout | → `/login`, session removed |
