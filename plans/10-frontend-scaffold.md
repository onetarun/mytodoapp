# Task 10 — Angular Scaffold & Core Setup

**Depends on:** 09
**References:** FR-5, NFR-10, NFR-18, NFR-19 · ARCHITECTURE §7 (client storage), §10
**Goal:** An Angular app that talks to the API, attaches the JWT automatically, protects routes and handles expired sessions and errors.

## Steps

### Project
- [ ] `ng new todo-app` inside `frontend/` (standalone components, routing, SCSS).
- [ ] Folder structure: `src/app/core/`, `src/app/shared/`, `src/app/features/{auth,tasks,categories,dashboard,notifications}/`.
- [ ] `environment.ts` / `environment.development.ts` with `apiBaseUrl` (e.g. `/api/v1`).
- [ ] `proxy.conf.json` forwarding `/api` to the HTTPS API (`secure: false` for the dev cert); wire it in `angular.json` `serve` options.
- [ ] Choose the UI kit (Angular Material **or** plain SCSS) and set base styles: font, spacing, colors with good contrast.

### Core (`core/`)
- [ ] `models/` — TypeScript interfaces mirroring the API DTOs (AuthResponse, TaskListItem, TaskDetail, SubTask, Category, DashboardSummary, Notification, Priority, TaskStatusFilter).
- [ ] `AuthService` — `login()`, `register()`, `logout()`, `token` and `currentUser` (signal), `isLoggedIn()` that also checks `expiresAt`. Store `{ token, expiresAt, user }` in **`sessionStorage`**.
- [ ] `authInterceptor` (functional) — adds `Authorization: Bearer <token>` to API calls.
- [ ] `errorInterceptor` — on `401`: logout + redirect to `/login`; on `429`: "Too many requests, try again shortly"; on `0`/`5xx`: generic friendly message. Read `title`/`errors` from ProblemDetails for `400`.
- [ ] `authGuard` (functional) — redirect to `/login` when not logged in; `guestGuard` — redirect logged-in users away from login/register.
- [ ] `ToastService` / snackbar for success and error messages.
- [ ] Auto-logout timer: when `expiresAt` passes, log out and redirect (NFR-10).

### Shell & routing
- [ ] `AppShell` layout: header (app name, nav links: Dashboard, Tasks, Categories; bell placeholder; user name; Logout) and main content area.
- [ ] Routes (lazy-loaded feature routes): `/login`, `/register` (guestGuard); `/dashboard`, `/tasks`, `/tasks/:id`, `/categories` (authGuard, inside the shell); `''` → `/dashboard`; `**` → `/dashboard`.
- [ ] Placeholder components for each route.

## Definition of Done

- [ ] `ng serve` runs; `http://localhost:4200` loads without console errors.
- [ ] Opening `/tasks` while logged out redirects to `/login`.
- [ ] An API call from the app reaches the backend through the proxy (no CORS error).
- [ ] `ng build` succeeds.
