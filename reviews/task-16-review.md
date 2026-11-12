# Review — Task 16: Responsive & Accessibility Polish

**Date:** 2026-11-12 · **Branch:** task/16-responsive-accessibility · **Verdict:** ✅ PASS (Edge smoke test ⚠️ left to the client, as agreed)

## Summary
The app now works from 360 px to 1920 px without horizontal scrolling. Below 768 px the header collapses into a "Main menu" and the bell stays visible. Dashboard cards use 1, 3 or 5 columns. The bell badge shows the number. Keyboard users get a visible focus ring on every Material control, a working skip link, and focus returns to the Edit button after the edit dialog closes. Lighthouse Accessibility is 100 on Login, Dashboard and Tasks, and with 1,000 tasks both pages load in under 1 s on the production build. The Edge smoke test is the client's, as decided on 2026-11-12.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Lint | `npx ng lint` | ✅ "All files pass linting." |
| Prettier | `npx prettier --check "src/**/*.{ts,html,scss}"` | ✅ "All matched files use Prettier code style!" |
| Strict mode | `tsconfig.json` | ✅ `strict` and `strictTemplates` on (unchanged) |
| Build | `npx ng build` | ✅ no warnings; initial 559.27 kB raw / 132.92 kB transfer (under the 650 kB budget) |
| Lazy routes | `npx ng build --verbose` | ✅ lazy chunks: `auth-routes`, `dashboard-routes`, `tasks-routes`, `categories-routes` and every page component |
| Backend checks | — | N/A: no `backend/` changes |
| Code searches (changed files) | Grep: `innerHTML`, `outerHTML`, `bypassSecurityTrust`, `console.log`, `TODO`, `FIXME`, `xit(`/`fit(`/`fdescribe`/`xdescribe` | ✅ no matches |
| Comments | Grep `//`, `<!--` | ✅ only explanatory comments (and the Angular CLI's own comments in `styles.scss`); no commented-out code |
| Packages | `git diff main...HEAD -- package.json package-lock.json` | ✅ no changes. Lighthouse 12.8.2 ran from a scratch folder outside the project, as approved (GUARDRAILS Appendix A "Audit tools", 2026-11-12) |
| Lighthouse (desktop, production build, 1,000-task user) | Lighthouse 12.8.2 via puppeteer, logged in | ✅ Accessibility 100 / 100 / 100 (Login / Dashboard / Tasks); Performance 99 / 100 / 93 |
| Lighthouse (mobile emulation) | same, mobile config, accessibility only | ✅ 100 / 100 / 100 |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Step: every page at 360/768/1280/1920 | ✅ | Headless Chrome 157: Login, Register, Dashboard, Tasks, Task detail, Categories, edit dialog, bell menu at all 4 widths; screenshots reviewed |
| Step: header collapses, bell visible | ✅ | `app-shell.component.html/.scss`: `.wide-only` / `.narrow-only` at 767.98 px; "Main menu" holds the user name, Dashboard, Tasks, Categories and Logout |
| Step: rows wrap, ≥ 44 px targets | ✅ | Icon buttons and checkboxes 48 px; task and upcoming title links `min-height: 44px` |
| Step: dashboard 1 / 2–3 / 5 columns | ✅ | `dashboard-page.component.scss`: 1 column below 600 px, 3 from 600 px, 5 from 1024 px; screenshots at 360 / 768 / 1280 |
| Step: dialogs fit, no horizontal scroll | ✅ | `scrollWidth − clientWidth = 0` on every page and width (was 244 px at 360 px before) |
| Step: tab order, focus ring, no traps, dialog focus | ✅ | Keyboard script with real key presses at 1280 and 360 px: every stop has a visible ring (`mat.strong-focus-indicators`); edit and confirm dialogs keep focus inside; Esc on the edit dialog returns focus to "Edit …" (`disabledInteractive`); the skip link focuses `<main>` |
| Step: labels and `aria-label`s | ✅ | Lighthouse `label`, `button-name`, `link-name` pass |
| Step: contrast ≥ 4.5:1 | ✅ | Lighthouse `color-contrast` passes on all pages, desktop and mobile |
| Step: not color alone | ✅ | Overdue = red + warning icon + "Overdue"; priority = text badge; completed = ticked + strike-through + label; bell overdue icons now red too |
| Step: Lighthouse Accessibility ≥ 90 | ✅ | 100 / 100 / 100 |
| Step: bundle sizes, lazy routes | ✅ | See automated checks |
| Step: 1,000 tasks < 2 s | ✅ | User `task16-perf@example.test` with 1,000 tasks; production build served with gzip; cold reload with cache off: task list 928 / 971 / 946 ms until all 1,000 rows show, dashboard 356 / 308 / 362 ms |
| Step: Chrome and Edge smoke test | ✅ Chrome / ⚠️ Edge | Chrome 157: full keyboard-only flow passed. Edge 157: the client does this check (decision 2026-11-12) |
| Step: UI consistency, English only | ✅ | Shared spacing tokens, Roboto, Material buttons; all text English |
| DoD: no layout breaks at 4 widths | ✅ | 0 px overflow everywhere; screenshots reviewed |
| DoD: Lighthouse Accessibility ≥ 90 | ✅ | 100 on all three |
| DoD: full flow keyboard only | ✅ | Login → skip link → quick add → Space to complete → edit dialog → delete with confirm → detail → bell → logout, also through the mobile Main menu |
| DoD: < 2 s with 1,000 tasks | ✅ | < 1 s / < 0.4 s |
| DoD: Chrome and Edge smoke tests | ⚠️ NOT VERIFIED (Edge) | Chrome passed; Edge is with the client |

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–15 ✅ in the index |
| G-PROC-2 | ✅ | Client ran `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 to 6 | ✅ | — |
| G-PROC-7 | ✅ | Lighthouse and Edge raised with the client before use; decisions recorded |
| G-SCOPE-1 to 3 | ✅ | Only NFR-2, NFR-19 to NFR-25 work. Also fixed the Task 15 bell badge (it showed a dot, not the count required by FR-19) |
| G-SCOPE-4 | ✅ | Earlier files changed only to meet this task's NFRs (listed below) |
| G-GIT-1 | ✅ | `task/16-responsive-accessibility` from `main` at `7167bb8` |
| G-GIT-2 | ✅ | All 6 commits start with `[Task 16]` |
| G-GIT-3, 4 | ✅ | Not merged; no remote |
| G-GIT-5 | ✅ | Nothing ignored tracked; the temporary `.claude/launch.json` and the scratch audit scripts were never committed |
| G-DEP-1 to 3 | ✅ | No project packages added; `lighthouse` approved as an `npx` audit tool and added to Appendix A + B |
| G-SEC-1, 2 | ✅ | No secrets in the diff; test passwords only passed as environment variables to local scripts |
| G-SEC-3, 4 | N/A | No backend or request changes |
| G-SEC-5 | ✅ | No `innerHTML` |
| G-SEC-6 to 8 | N/A | Backend unchanged |
| G-CODE-1, 2 | ✅ | Build, lint, Prettier clean |
| G-CODE-3, 4 | N/A | Backend only |
| G-CODE-5, 6 | ✅ | No new files |
| G-CODE-7 | ✅ | No dead code |
| G-CODE-8 | ✅ | Breakpoints are layout values in SCSS, not business rules |
| G-TEST-1 to 4 | N/A | No frontend tests in v1; backend unchanged |
| G-DB-1 to 4 | N/A | No schema changes. The 1,000 test tasks were created through the API |
| G-DESTRUCT-1 to 4 | ✅ | Nothing destructive. The API ran once with `RateLimiting__DefaultPermitLimit=5000` as an environment variable for seeding; no config file changed |

## Issues to fix (FAIL only)
None.

## Not verified
- **Edge 157 smoke test** — left to the client (decision 2026-11-12). Please run: login → add a task → tick it → open detail and add a checklist item → bell → logout, at a normal and a narrow window.
- **Real screen reader (NVDA/Narrator)** — checked through Lighthouse/axe rules and the DOM only.
- **Focus after deleting a task** — the confirm dialog cannot return focus to the deleted row, so focus goes to the page body. Not a trap; Tab starts again from the top.
- **Load time on a real network** — measured on localhost with gzip (unthrottled timings < 1 s; Lighthouse uses simulated desktop throttling). A deployed server and slower networks will add latency.
- **Test data in `TodoAppDb`** — `task16-perf@example.test` with 1,000 tasks, plus the users from Tasks 14 and 15.

## Files changed outside this task's scope
- `frontend/todo-app/src/app/core/layout/app-shell/*` (Task 10/15) — responsive header, skip-link fix.
- `frontend/todo-app/src/app/features/notifications/notification-bell/*` (Task 15) — badge shows the count, red overdue icons.
- `frontend/todo-app/src/app/features/dashboard/dashboard-page/dashboard-page.component.scss` (Task 15) — card columns, 44 px links.
- `frontend/todo-app/src/app/features/tasks/task-list-page/*`, `task-checklist/*.html`, `task-form/*.scss` (Tasks 13/14) — `disabledInteractive`, 44 px title links, one-line submit button.
- `frontend/todo-app/src/app/features/categories/categories-page/categories-page.component.html` (Task 12) — `disabledInteractive`.
- `frontend/todo-app/src/styles.scss` — Material strong focus indicators.
- `GUARDRAILS.md` — Appendix A "Audit tools" + Appendix B entry (client approval).
- `plans/00-INDEX.md` — Task 16 status. Also fixed a status error: unanchored status updates had also matched the "Depends on" column, so Task 16 showed "✅ Done" on `main` after the Task 15 merge. This branch sets it back to "🔄 In progress" (now "🟡 Awaiting merge") and Task 17 back to "☐ Not started". `main` shows the wrong Task 16 status until this branch is merged.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-12 | ✅ PASS (Edge ⚠️ client check) |
