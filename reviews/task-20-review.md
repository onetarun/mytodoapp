# Review — Task 20: Square Buttons with Rounded Corners

**Date:** 2026-11-12 · **Branch:** task/20-square-buttons · **Verdict:** ✅ PASS

## Summary
GitHub issue onetarun/mytodoapp#1 asked for square buttons with rounded corners instead of the oval (pill) Material 3 default. BRD v1.4 adds NFR-22a, and one global `mat.button-overrides` in `src/styles.scss` sets the container shape of text, filled, outlined, tonal and protected buttons to 8 px. Build, lint and Prettier pass, and the browser shows `border-radius: 8px` on the login button.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build | `npx ng build` | ✅ "Application bundle generation complete", no warnings or errors |
| Lint | `npx ng lint` | ✅ "All files pass linting." |
| Prettier | `npx prettier --check "src/**/*.{ts,html,scss}"` | ✅ "All matched files use Prettier code style!" |
| Strict mode | `tsconfig.json` | ✅ `"strict": true`, `"strictTemplates": true` |
| Frontend unit tests | `npx ng test` | N/A: the Angular project has no `test` target ("Cannot determine project or target for command") |
| Backend checks | — | N/A: no backend file changed |
| Code searches (TODO, FIXME, `console.log`, `innerHTML`, `bypassSecurityTrust`, `::ng-deep`, secrets) | Grep on `git diff main...HEAD` | ✅ No matches in code (only the task file's own "no `::ng-deep`" text) |
| Compiled CSS | grep `dist/todo-app/browser/styles-*.css` | ✅ five `container-shape: 8px` declarations |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| BRD v1.4: NFR-22a | ✅ | `BRD.md` header and §5.6 |
| Index: Task 20 added | ✅ | `plans/00-INDEX.md` row 20 and dependency chain |
| Global `mat.button-overrides` with 8 px shapes | ✅ | `frontend/todo-app/src/styles.scss` lines 21–31; token names confirmed in `@angular/material` 22.2.7 |
| Icon buttons stay circular | ✅ | `.mat-mdc-icon-button` uses `--mat-icon-button-container-shape`, which is not overridden |
| No component overrides, no `::ng-deep`, no new packages | ✅ | Diff touches only `styles.scss` in the frontend; `package.json` unchanged |
| Build, lint, Prettier pass | ✅ | See automated checks |
| Browser check | ✅ | `ng serve` on `http://localhost:4300/login`: "Log in" button computed `border-radius: 8px`, screenshot shows a rounded rectangle |
| No backend change | ✅ | `git diff --stat main...HEAD` has no `backend/` file |

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–19 are ✅ Done in the index |
| G-PROC-2 | ✅ | Client said "Haan, fix karo" for issue #1 |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Verdict is PASS |
| G-PROC-5 | ✅ | Not merged; client approval to be requested |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | No blocker |
| G-SCOPE-1 | ✅ | Change matches NFR-22a and the task file |
| G-SCOPE-2 | ✅ | BRD updated to v1.4 before the code change, in the same branch |
| G-SCOPE-3 | ✅ | No Phase 2 item |
| G-SCOPE-4 | ✅ | Only task files, BRD and the index changed |
| G-GIT-1 | ✅ | `task/20-square-buttons` created from `main` (f46b2ab) |
| G-GIT-2 | ✅ | Both commits start with `[Task 20]` |
| G-GIT-3 | ✅ | Not merged |
| G-GIT-4 | ✅ | No upstream configured, nothing pushed |
| G-GIT-5 | ✅ | No `bin/`, `obj/`, `node_modules/`, `logs/` or `.bak` files tracked |
| G-DEP-1 to G-DEP-3 | ✅ | No package added (`npm ci` only installed the existing lockfile) |
| G-SEC-1 to G-SEC-8 | ✅ / N/A | No secrets, logging, API or Angular template change |
| G-CODE-1 | ✅ | Build has no warnings; strict mode on |
| G-CODE-2 | ✅ | Lint and Prettier clean |
| G-CODE-3 to G-CODE-6 | N/A | No backend or TypeScript change |
| G-CODE-7 | ✅ | No dead code, TODO or `console.log` |
| G-CODE-8 | ✅ | 8 px is a visual style value defined once in NFR-22a, not a business rule |
| G-TEST-1 | ✅ | No skipped tests; no test files changed |
| G-TEST-2 to G-TEST-4 | N/A | No service method added; backend unchanged |
| G-DB-1 to G-DB-4 | N/A | No database change |
| G-DESTRUCT-1 to G-DESTRUCT-4 | ✅ | None happened. The temporary `.claude/launch.json` used for the browser check was created and deleted in this task and never committed |

## Issues to fix (FAIL only)
None.

## Not verified
- Pages behind login (task list, categories, dashboard) were not opened in the browser, because no backend was running. They use the same global button tokens, so the same 8 px shape applies.
- The keyboard focus ring shape on the new buttons was not checked by eye.

## Files changed outside this task's scope
- `BRD.md` — v1.4, NFR-22a (G-SCOPE-2).
- `plans/00-INDEX.md` — new task row.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-12 | ✅ PASS |
