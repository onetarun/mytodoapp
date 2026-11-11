# Review — Task 01: Environment Setup

**Date:** 2026-11-11 · **Branch:** task/01-environment-setup · **Verdict:** ✅ PASS

## Summary
Every tool the project needs is installed and was verified by running its version or status command. The task changes only `plans/00-INDEX.md` (status) and `plans/01-environment-setup.md` (ticked steps, instance name, edition, verification table). No application code exists yet, so most code, security and test guardrails do not apply.

## Automated checks
| Check | Command | Result |
|---|---|---|
| .NET SDK | `dotnet --version` | ✅ `10.0.403` |
| EF Core CLI | `dotnet ef --version` | ✅ `10.0.11` |
| Node.js / npm | `node -v` / `npm -v` | ✅ `v24.18.0` / `11.16.0` |
| Angular CLI | `ng version` | ✅ `22.0.7` |
| HTTPS dev certificate | `dotnet dev-certs https --check --trust` | ✅ trusted certificate found, exit code 0 |
| SQL Server service | `Get-Service MSSQL$SQLEXPRESS` | ✅ `Running` |
| SQL Server connection | `sqlcmd -S "localhost\SQLEXPRESS" -E -C -Q "SELECT SERVERPROPERTY('Edition') ..."` | ✅ connected; `Express Edition (64-bit)`, `16.0.1000.6` |
| Editors | `vswhere -products * -property displayName`; `code --list-extensions` | ✅ Visual Studio Community 2026, SSMS 21; VS Code with `angular.ng-template` |
| Backend build / format / tests | — | N/A: `backend/` does not exist yet |
| Frontend lint / prettier / build | — | N/A: `frontend/todo-app/` does not exist yet |
| Code searches (secrets, raw SQL, innerHTML, etc.) | Grep over changed files | ✅ only Markdown files changed; no matches |
| Branch | `git branch --show-current` | ✅ `task/01-environment-setup`, created from `main` (6d4f149) |
| Commit messages | `git log --oneline main..HEAD` | ✅ `dfa3fe7 [Task 01] Record verified development environment` |
| Tracked build/secret files | `git ls-files` filtered for `bin/`, `obj/`, `node_modules/`, `logs/`, `.bak` | ✅ none |
| Remotes / push | `git remote -v` | ✅ no remote configured; nothing pushed |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| `dotnet --version`, `dotnet ef --version`, `node -v`, `ng version` print versions | ✅ | See automated checks above |
| SSMS / Azure Data Studio connects to the local SQL Server instance | ⚠️ NOT VERIFIED (GUI) | SSMS 21 is installed and the instance accepts Windows-auth connections (`sqlcmd` query succeeded). Opening the connection inside the SSMS GUI was not done by the AI. |
| HTTPS dev certificate is trusted | ✅ | `dotnet dev-certs https --check --trust` exit code 0 |
| SQL Server edition and instance name recorded in the task file | ✅ | `plans/01-environment-setup.md` Notes: `localhost\SQLEXPRESS`, Express |

All 10 Steps are ticked; each is backed by a row in the task file's verification table.

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Task 01 is the first task |
| G-PROC-2 | ✅ | Client ran `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 | N/A | Not a FAIL |
| G-PROC-5 | ✅ | Merge waits for client approval |
| G-PROC-6 | N/A | After merge |
| G-PROC-7 | ✅ | Missing git identity was raised to the client; client set it before the first commit |
| G-SCOPE-1 | ✅ | Only task-file and index updates |
| G-SCOPE-2 | ✅ | No new feature |
| G-SCOPE-3 | ✅ | No Phase 2 work |
| G-SCOPE-4 | ✅ | Only Task 01 files and the index status changed |
| G-GIT-1 | ✅ | `task/01-environment-setup` from `main` |
| G-GIT-2 | ✅ | `[Task 01] ...` |
| G-GIT-3 | ✅ | Not merged yet |
| G-GIT-4 | ✅ | No remote |
| G-GIT-5 | ✅ | `.gitignore` covers `bin/`, `obj/`, `node_modules/`, `logs/`, `*.bak`; none tracked |
| G-DEP-1 to 3 | ✅ | No packages added. Global tools (`dotnet-ef`, Angular CLI) were already installed; `dotnet-ef` is on Appendix A |
| G-SEC-1 to 8 | N/A | No code yet; no secrets in the changed Markdown files |
| G-CODE-1 to 8 | N/A | No code yet |
| G-TEST-1 to 4 | N/A | No code yet |
| G-DB-1 to 4 | N/A | No database objects created |
| G-DESTRUCT-1 to 4 | ✅ | No destructive action; nothing deleted or reset |

## Issues to fix (FAIL only)
None.

## Not verified
- SSMS GUI connection to `localhost\SQLEXPRESS`: the AI cannot drive the SSMS window. The instance itself accepts connections (verified with `sqlcmd`). Client: open SSMS, connect to `localhost\SQLEXPRESS` with Windows Authentication, and confirm it works.
- VS Code has no C# Dev Kit extension. This does not block the task because Visual Studio Community 2026 is installed (the step allows "or Visual Studio"). Install C# Dev Kit only if you want to use VS Code for C#.

## Files changed outside this task's scope
- `plans/00-INDEX.md` — status of Task 01 set to 🔄 In progress (required by the process).

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-11 | ✅ PASS |
