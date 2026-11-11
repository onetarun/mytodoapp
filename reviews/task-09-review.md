# Review — Task 09: Backend Test Coverage Gate

**Date:** 2026-11-11 · **Branch:** task/09-backend-test-coverage · **Verdict:** ✅ PASS

## Summary
The backend is confirmed complete and stable before frontend work. Service-layer coverage is **100 % lines (401/401) and 92.4 % branches**, against a 60 % gate, so no extra tests were needed. Swagger lists all 18 ARCHITECTURE §6 endpoints (plus health) with request/response schemas and status codes. The only code change is that the Bearer lock now appears only on protected endpoints: before, register and login showed it too. A full end-to-end pass ran without errors, and the logs show no errors or warnings.

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ `0 Warning(s)`, `0 Error(s)` |
| Nullable + warnings-as-errors | csproj inspection (unchanged) | ✅ |
| Format | `dotnet format --verify-no-changes` | ✅ exit code 0 |
| Tests | `dotnet test` | ✅ 137 passed, 0 failed, 0 skipped |
| Coverage (G-TEST-4) | `dotnet test --collect:"XPlat Code Coverage"`, Cobertura summed per namespace | ✅ `TodoApp.Api.Services` 401/401 lines = **100 %**, branches 85/92 = 92.4 % |
| Vulnerable packages | `dotnet list package --vulnerable --include-transitive` | ✅ none |
| Code searches on changed files | grep secrets, logging, raw SQL, TODO, `DateTime.Now`, magic numbers | ✅ only comment hits. `"401"` is a named constant (HTTP status key) |
| `[AllowAnonymous]` | grep | ✅ unchanged (`AuthController`, `HealthController`); the filter only reads it |
| One public type per file | count | ✅ |
| Branch / commits | `git log main..HEAD --oneline` | ✅ `task/09-backend-test-coverage` from `main` (72a90b5); 2 commits, both `[Task 09] ...` |
| Remote / tracked junk | `git remote -v`, `git ls-files` | ✅ no remote; `TestResults/` and `logs/` not tracked (`.gitignore`) |
| Packages / migrations | diff | ✅ no changes |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| All backend tests pass | ✅ | 137/137 |
| Service-layer coverage ≥ 60 % (number recorded) | ✅ | 100 % lines / 92.4 % branches, recorded in `plans/09-backend-test-coverage.md` |
| All 18 endpoints documented in Swagger | ✅ | `swagger.json` parsed: 19 operations (18 + health); every one has response codes and schemas, and bodies have request schemas; 16 locked, register/login/health unlocked (confirmed in a Swagger UI screenshot) |
| Manual Swagger pass completed with no errors | ✅ (same requests via `curl`) / ⚠️ not clicked through in the Swagger UI | register `201` → login `200` → category `201` → task `201` → subtasks `201` ×2 → tick `200` (progress `1/2`) → dashboard `200` → notifications `200` (1 DueSoon) → complete `200` → delete category `204` (task `categoryId: null`) → delete task `204` (list `[]`). Logs: 336 new lines, 0 `ERR`/`WRN` |

Steps check:
- Coverage run and readable summary. ✅ The summary comes from a script over the Cobertura file, because `reportgenerator` is not installed (see Not verified).
- Coverage < 60 % → add tests. N/A (100 %).
- Swagger review, lock except register/login. ✅ after the fix.
- `[ProducesResponseType]` for status codes. ✅ already on every action; `401` is added by the filter on protected endpoints.
- `logs/` checked. ✅
- Backend committed as a stable baseline. ✅ (`854579f`)

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–08 ✅ Done |
| G-PROC-2 | ✅ | `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Waiting for merge approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | `reportgenerator` missing was not a blocker (coverage read from Cobertura). Nothing was installed |
| G-SCOPE-1, 2 | ✅ | Swagger documentation only; no API behavior changed |
| G-SCOPE-3 | ✅ | — |
| G-SCOPE-4 | ✅ | `SwaggerExtensions.cs` (Task 02) changed, as the task's "lock except register/login" step requires |
| G-GIT-1 to 5 | ✅ | See automated checks |
| G-DEP-1 to 3 | ✅ | No packages added |
| G-SEC-1 to 8 | ✅ | Unchanged behavior; Swagger stays Development-only (`Program.cs`) |
| G-CODE-1 to 8 | ✅ | 0 warnings, format clean, one type per file, no dead code |
| G-TEST-1 | ✅ | 137/137 |
| G-TEST-2, 3 | ✅ | No new service methods; earlier coverage stands |
| G-TEST-4 | ✅ | 100 % ≥ 60 % |
| G-DB-1 to 4 | ✅ / N/A | No schema or data-access changes |
| G-DESTRUCT-1 to 4 | ✅ | Nothing dropped or deleted |

## Issues to fix (FAIL only)
None.

## Not verified
- **Swagger UI click-through:** the e2e pass sent the same requests with `curl`. The UI itself was checked by screenshot (locks, Authorize button) but not clicked through. Client: one pass in `/swagger` is recommended before frontend work.
- **`reportgenerator` HTML report:** `dotnet-reportgenerator-globaltool` (Appendix A) is not installed, so there is no HTML report. Install it if you want one: `dotnet tool install --global dotnet-reportgenerator-globaltool`.
- **429 response schema on login:** Swagger shows `ProblemDetails` for `429`, but the rate limiter actually returns an empty body. This is cosmetic.
- **Verification data in `TodoAppDb`:** user `task09-e2e-…@example.test` (its task and category are soft deleted). Earlier perf data is still there (2 × 1,000 tasks).

## Files changed outside this task's scope
- `backend/TodoApp.Api/Extensions/SwaggerExtensions.cs` (Task 02) — the global security requirement was replaced by the operation filter (required by the "lock except register/login" step).
- `plans/00-INDEX.md` — Task 09 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
