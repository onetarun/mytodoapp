# Review — Task 19: Azure Deployment

**Date:** 2026-11-12 · **Branch:** task/19-azure-deployment · **Verdict:** ❌ FAIL

## Summary
The scope change is documented first (BRD v1.3, ARCHITECTURE AD-6), the API serves the Angular build with a correct SPA fallback, and the local production-mode run works end to end. One defect blocks the PASS: the deploy script's smoke check only handles the Windows PowerShell 5.1 exception type, while `azure-common.ps1` says the scripts support "5.1+".

## Automated checks
| Check | Command | Result |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` | ✅ 0 Warning(s), 0 Error(s) |
| Nullable + warnings-as-errors | inspect `*.csproj` | ✅ unchanged, both projects set |
| Format | `dotnet format --verify-no-changes` | ✅ exit 0 |
| Tests | `dotnet test` | ✅ 137 passed, 0 failed, 0 skipped |
| Coverage | `dotnet test --collect:"XPlat Code Coverage"` (Services classes) | ✅ 802/802 lines = 100% |
| Frontend lint / Prettier / build | `npm run lint`, `npm run format:check`, `npm run build` | ✅ all pass (frontend files unchanged in this task) |
| Strict mode | `tsconfig.json` | ✅ unchanged |
| PowerShell parse | `[Parser]::ParseFile` on the 3 scripts | ✅ 0 errors |
| Local production run | published output + `wwwroot`, `ASPNETCORE_ENVIRONMENT=Production` | ✅ `/` 200, `/tasks` 200, `/api/v1/nope` 404 ProblemDetails, no-token API 401, register 201, login + categories 200, browser login → `/tasks` |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Backend build/format/tests | ✅ | see above |
| Frontend build/lint/format | ✅ | see above |
| Local production-mode run | ✅ | plan Notes, commands above |
| Scripts parse | ✅ | 0 parser errors |
| No secrets in git | ✅ | grep: the only `Password=` is built from a generated variable in `ops/azure-provision.ps1:91`; no literal secrets |
| Client ran both scripts, opened the Azure URL | ⚠️ NOT VERIFIED | Client action (Azure subscription + `az login`) |

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–18 ✅ in the index |
| G-PROC-2 | ✅ | Client said "Haan, shuru karo" for the Azure plan |
| G-PROC-3..7 | ✅ | This review |
| G-SCOPE-1/2 | ✅ | BRD v1.3 (NFR-5a, NFR-29–31) and AD-6 committed before code (`0f1235d`) |
| G-SCOPE-3 | ✅ | CI/CD moved to the Phase 2 backlog, not built |
| G-SCOPE-4 | ✅ | `Program.cs` changed (needed, listed below) |
| G-GIT-1/2/4/5 | ✅ | branch `task/19-azure-deployment` from `main`; both commits `[Task 19] …`; no upstream; no build output tracked |
| G-GIT-3 | N/A | Not merged |
| G-DEP-1..3 | ✅ | No package changes. Azure CLI is an external tool, not a project dependency |
| G-SEC-1 | ✅ | Secrets generated at run time, stored only in App Service settings; temp settings file deleted in `finally` |
| G-SEC-2 | ✅ | Scripts never print secrets; `Invoke-Az` error messages include only the first two arguments |
| G-SEC-3/4/5/8 | ✅ | No change |
| G-SEC-6 | ✅ | Two `.AllowAnonymous()` fallbacks serve only `index.html` and a 404; no controller became anonymous. Approved in ARCHITECTURE §13 |
| G-SEC-7 | ✅ | Unknown API path returns ProblemDetails 404 without internals |
| G-CODE-1..7 | ✅ | New constants class follows one-type-per-file; no TODO/dead code |
| G-CODE-8 | ✅ | Route strings as named constants; backup retention as a named variable |
| G-TEST-1/4 | ✅ | 137/137, Services 100% |
| G-TEST-2/3 | N/A | No new service methods |
| G-DB-1..4 | ✅ | No migration changes |
| G-DESTRUCT-1..4 | ✅ | Nothing dropped or deleted. `TodoAppDb_DeployTest` was created and left in place |

## Issues to fix (FAIL only)
1. `ops/azure-deploy.ps1:55` — `Get-HttpStatus` catches only `System.Net.WebException`. In PowerShell 7 (`pwsh`), `Invoke-WebRequest` throws `HttpResponseException` for 4xx, so the expected `400` from `POST /api/v1/auth/login {}` aborts the deploy with an error after a successful zip deploy. `azure-common.ps1:1` states "Windows PowerShell 5.1+". — correctness of the DoD "smoke check" step.

## Not verified
- The `az` commands against a real subscription — Azure CLI is not installed here and the subscription belongs to the client.
- The `ASPNETCORE_FORWARDEDHEADERS_ENABLED` effect on App Service (HTTPS redirect, per-IP login limit) — needs the real App Service front end.

## Observations (not blocking)
- `azure-deploy.ps1` calls `https://api.ipify.org` to find this machine's public IP for the temporary firewall rule. That sends the IP to a third party; the rule is removed after the migration.
- Unknown non-API paths with a file extension (e.g. `/missing.js`) return 401 from the authorization fallback policy, as before this task.

## Files changed outside this task's scope
- `backend/TodoApp.Api/Program.cs` — static files and SPA fallback (AD-6).
- `README.md`, `plans/00-INDEX.md` — deploy section and the new task row.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-12 | ❌ FAIL (PowerShell 7 smoke-check exception) |
