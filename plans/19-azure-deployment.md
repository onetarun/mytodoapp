# Task 19 — Azure Deployment

**Depends on:** 18
**References:** NFR-5a, NFR-12, NFR-14, NFR-17, NFR-29 to NFR-31 · ARCHITECTURE §13 (AD-6)
**Goal:** The app runs on Azure App Service with Azure SQL Database, deployed by two scripts the client runs after `az login`.

## Steps

### Docs (scope change first, G-SCOPE-2)
- [ ] BRD v1.3: hosting row, NFR-5a, NFR-29 to NFR-31, Assumption 2 resolved, CI/CD in the Phase 2 backlog.
- [ ] ARCHITECTURE: AD-6 in the decision summary and §13.

### Backend
- [ ] `Program.cs`: serve `wwwroot` (`UseDefaultFiles`, `UseStaticFiles`) before authentication, `MapFallbackToFile("index.html")` for client routes (anonymous).
- [ ] Unknown `/api/*` paths return 404, never `index.html`.
- [ ] No new packages (G-DEP-1).

### Ops scripts
- [ ] `ops/azure-provision.ps1` (one-time): resource group, App Service plan + Linux web app (.NET 10), Azure SQL server + database, firewall rules, HTTPS-only, TLS 1.2, app settings (connection string, JWT key, CORS origin, log path, forwarded headers), 7-day backup retention. Generated secrets are never printed or written to disk.
- [ ] `ops/azure-deploy.ps1` (per release): Angular production build, `dotnet publish`, copy the Angular build into `wwwroot`, `dotnet ef database update` against Azure SQL, zip deploy, smoke check of `/` and `/api/v1/auth/login`.
- [ ] Both scripts stop on the first error and refuse to run without `az login`.

### Docs for the client
- [ ] README: "Deploy to Azure" section (prerequisites, commands, cost note, redeploy, logs, restore).

## Definition of Done

- [ ] Backend builds with zero warnings, `dotnet format --verify-no-changes` clean, all tests pass.
- [ ] Frontend `npm run build`, lint and format check pass.
- [ ] Local production-mode run of the published output (API + `wwwroot`): `/` and a deep link such as `/tasks` return `index.html`, `/api/v1/does-not-exist` returns 404, register and login work.
- [ ] Scripts parse without errors (PowerShell parser check).
- [ ] No secrets in git (G-SEC-1).
- [ ] The client has run both scripts and opened the Azure URL (recorded below).

## Notes

- Azure URL: _filled after the client's first deploy_
- Region / tiers used: _filled after the client's first deploy_
