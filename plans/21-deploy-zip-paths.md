# Task 21 — Fix Deploy Zip Paths

**Depends on:** 20
**References:** NFR-29 · ARCHITECTURE §13 (AD-6) · Task 19
**Goal:** `ops/azure-deploy.ps1` creates a zip that Azure App Service (Linux) can extract when the script runs in Windows PowerShell 5.1.

## Problem

On 2026-11-12 the deploy of Task 20 failed at "Zip deploy to App Service" (Kudu status 400). `[IO.Compression.ZipFile]::CreateFromDirectory` on Windows PowerShell 5.1 (.NET Framework) writes entry names with `\` (for example `wwwroot\index.html`). Kudu's rsync on Linux treats them as file names, not folders (`failed to stat "/home/site/wwwroot/wwwroot\index.html"`), so files in subfolders were not deployed.

## Steps

- [x] `ops/azure-deploy.ps1`: replace `ZipFile::CreateFromDirectory` with a `New-DeployZip` function that adds every file with an entry name relative to the publish folder, using `/` as the separator.
- [x] Uses only `System.IO.Compression` APIs that exist in Windows PowerShell 5.1 and PowerShell 7. No new tools or packages (G-DEP-1). *(Tested in 5.1 only; PowerShell 7 is not installed on this machine.)*
- [x] No other behavior of the script changes.

## Definition of Done

- [x] Script parses without errors (PowerShell parser check). *(Windows PowerShell 5.1 parser: 0 errors in `azure-deploy.ps1`, `azure-common.ps1`, `azure-provision.ps1`.)*
- [x] Local test in Windows PowerShell 5.1: a zip built by `New-DeployZip` from a folder with nested files has only `/` in entry names, contains every file, and extracts to the same tree. *(2026-11-12, PowerShell 5.1.26100: 5 files including a hidden file and three nested levels; entries `TodoApp.Api.dll`, `wwwroot/index.html`, `wwwroot/media/font.woff2`, `runtimes/linux-x64/native/lib.so`, `.hidden`; 0 backslash entries; extracted tree matches. The old `CreateFromDirectory` call on the same machine produced `wwwroot\index.html`, confirming the cause.)*
- [x] No secrets in git (G-SEC-1).
- [x] After merge: the deploy script runs end to end against `mytodoapp-onetarun` and the smoke check passes (recorded below).

## Notes

- Deploy result (2026-11-12, from `main` at ee8c3d1, Windows PowerShell 5.1): all steps ran; smoke check `GET / -> 200, GET /tasks -> 200, POST /api/v1/auth/login {} -> 400`. In the browser, the "Log in" button on https://mytodoapp-onetarun.azurewebsites.net/login has computed `border-radius: 8px` (Task 20 is live).
