# Review — Task 21: Fix Deploy Zip Paths

**Date:** 2026-11-12 · **Branch:** task/21-deploy-zip-paths · **Verdict:** ✅ PASS (deploy to Azure is done after merge)

## Summary
The Task 20 deploy failed because the zip that `ops/azure-deploy.ps1` built in Windows PowerShell 5.1 had `\` in its entry names. Linux App Service could not extract files in subfolders. A new `New-DeployZip` function writes every entry relative to the publish folder with `/` as the separator. A local test in PowerShell 5.1 shows 0 backslash entries and the same file tree after extraction. The end-to-end deploy is a post-merge DoD item and is listed under Not verified.

## Automated checks
| Check | Command | Result |
|---|---|---|
| PowerShell parser | `[Parser]::ParseFile` on `ops/azure-deploy.ps1`, `azure-common.ps1`, `azure-provision.ps1` (Windows PowerShell 5.1.26100) | ✅ 0 errors in each file |
| Zip function test | scratchpad `test-zip.ps1`: extract `New-DeployZip` from the script AST, zip a 5-file tree with three nested levels and a hidden file, inspect and extract | ✅ entries `.hidden`, `runtimes/linux-x64/native/lib.so`, `TodoApp.Api.dll`, `wwwroot/index.html`, `wwwroot/media/font.woff2`; backslash entries 0; 5 files / 5 entries; extracted tree matches |
| Root cause check | old `ZipFile::CreateFromDirectory` on the same machine | ✅ produced `wwwroot\index.html`, confirming the cause |
| Backend / frontend checks | — | N/A: no backend or frontend file changed |
| Code searches (secrets, TODO, FIXME, secret values in `Write-Host`) | Grep on `git diff main...HEAD` | ✅ no matches |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| `New-DeployZip` replaces `CreateFromDirectory`, `/` separators | ✅ | `ops/azure-deploy.ps1` lines 63–79 and line 127 |
| Only `System.IO.Compression` APIs; no new tools or packages | ✅ | Diff adds no tool or package; `ZipFile.Open`, `ZipArchiveMode.Create`, `ZipFileExtensions.CreateEntryFromFile` exist in .NET Framework 4.5+ and .NET |
| No other behavior change | ✅ | Diff touches only the zip step; build, migration, deploy and smoke-check code unchanged |
| Script parses without errors | ✅ | Parser check above |
| Local zip test in PowerShell 5.1 | ✅ | Zip function test above |
| No secrets in git | ✅ | No secret values in the diff |
| Deploy runs end to end, smoke check passes | ⚠️ | Post-merge item by design; not run yet |

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–20 are ✅ Done in the index |
| G-PROC-2 | ✅ | Client said "Haan, Task 21 karo" |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Verdict is PASS |
| G-PROC-5 | ✅ | Not merged; client approval to be requested |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | Blocker (failed deploy) was reported to the client before this task started |
| G-SCOPE-1 | ✅ | Bug fix inside NFR-29 / AD-6; no new feature |
| G-SCOPE-2 | N/A | No scope change, so no BRD update needed |
| G-SCOPE-3 | ✅ | No Phase 2 item (CI/CD not added) |
| G-SCOPE-4 | ✅ | Changes Task 19's `ops/azure-deploy.ps1`; this is the subject of the task (listed below) |
| G-GIT-1 | ✅ | `task/21-deploy-zip-paths` created from `main` (5a27a65 = merge base) |
| G-GIT-2 | ✅ | Both commits start with `[Task 21]` |
| G-GIT-3 | ✅ | Not merged |
| G-GIT-4 | ✅ | No upstream for this branch; nothing pushed |
| G-GIT-5 | ✅ | No build output, `node_modules`, logs or `.bak` files tracked |
| G-DEP-1 to G-DEP-3 | ✅ | No package added |
| G-SEC-1 | ✅ | No secrets in the diff |
| G-SEC-2 | ✅ | The new function prints nothing |
| G-SEC-3 to G-SEC-8 | N/A | No API or Angular change |
| G-CODE-1 to G-CODE-6 | N/A | No C# or TypeScript change |
| G-CODE-7 | ✅ | No dead code; the old two lines were removed, not commented out |
| G-CODE-8 | N/A | No business rule |
| G-TEST-1 to G-TEST-4 | N/A | No tests in scope; the script is tested by the local zip test |
| G-DB-1 to G-DB-4 | N/A | No database change |
| G-DESTRUCT-1 to G-DESTRUCT-4 | ✅ | None happened. The test folders were created and removed by the test itself |

## Issues to fix (FAIL only)
None.

## Not verified
- End-to-end deploy to `mytodoapp-onetarun` and the smoke check — runs after the client approves the merge (DoD item).
- PowerShell 7 — not installed on this machine. The function uses the same .NET APIs there, but it was only run in Windows PowerShell 5.1.
- Empty folders are not added to the zip, as before. The publish output has none that the app needs.

## Files changed outside this task's scope
- `ops/azure-deploy.ps1` — Task 19 file; the bug being fixed is in this file.
- `plans/00-INDEX.md` — new task row.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-12 | ✅ PASS (deploy after merge open) |
