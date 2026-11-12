# Task 19 — Azure Deployment

**Depends on:** 18
**References:** NFR-5a, NFR-12, NFR-14, NFR-17, NFR-29 to NFR-31 · ARCHITECTURE §13 (AD-6)
**Goal:** The app runs on Azure App Service with Azure SQL Database, deployed by two scripts the client runs after `az login`.

## Steps

### Docs (scope change first, G-SCOPE-2)
- [x] BRD v1.3: hosting row, NFR-5a, NFR-29 to NFR-31, Assumption 2 resolved, CI/CD in the Phase 2 backlog.
- [x] ARCHITECTURE: AD-6 in the decision summary and §13.

### Backend
- [x] `Program.cs`: serve `wwwroot` (`UseDefaultFiles`, `UseStaticFiles`) before authentication, `MapFallbackToFile("index.html")` for client routes (anonymous).
- [x] Unknown `/api/*` paths return 404, never `index.html`.
- [x] No new packages (G-DEP-1).

### Ops scripts
- [x] `ops/azure-provision.ps1` (one-time): resource group, App Service plan + Linux web app (.NET 10), Azure SQL server + database, firewall rules, HTTPS-only, TLS 1.2, app settings (connection string, JWT key, CORS origin, log path, forwarded headers), 7-day backup retention. Generated secrets are never printed or written to disk.
- [x] `ops/azure-deploy.ps1` (per release): Angular production build, `dotnet publish`, copy the Angular build into `wwwroot`, `dotnet ef database update` against Azure SQL, zip deploy, smoke check of `/` and `/api/v1/auth/login`.
- [x] Both scripts stop on the first error and refuse to run without `az login`.

### Docs for the client
- [x] README: "Deploy to Azure" section (prerequisites, commands, cost note, redeploy, logs, restore).

## Definition of Done

- [x] Backend builds with zero warnings, `dotnet format --verify-no-changes` clean, all tests pass. *(137/137)*
- [x] Frontend `npm run build`, lint and format check pass.
- [x] Local production-mode run of the published output (API + `wwwroot`): `/` and a deep link such as `/tasks` return `index.html`, `/api/v1/does-not-exist` returns 404, register and login work. *(2026-11-12, `http://localhost:5080`, `ASPNETCORE_ENVIRONMENT=Production`, database `TodoAppDb_DeployTest` migrated with the same env-var method the deploy script uses: `/` 200, `/tasks` 200, `/tasks/12` 200, `/api/v1/nope` 404 ProblemDetails, `/api/v1/tasks` without token 401, login `{}` 400, register 201, login + categories 200; login in the browser lands on `/tasks`.)*
- [x] Scripts parse without errors (PowerShell parser check). *(Windows PowerShell 5.1 parser: 0 errors in all three files.)*
- [x] No secrets in git (G-SEC-1).
- [ ] The client has run both scripts and opened the Azure URL (recorded below).

## Notes

- Azure URL: _filled after the client's first deploy_
- Region / tiers used: _filled after the client's first deploy_
- The `az` commands themselves can only be verified on the client's subscription; Azure CLI is not installed on this machine yet.
- Unknown non-API paths with a file extension (e.g. `/missing.js`) still return 401 from the authorization fallback policy, as before this task.
- Local test database `TodoAppDb_DeployTest` (one test user) was created by this task and left in place; it can be dropped with the client's approval (G-DESTRUCT-1).
