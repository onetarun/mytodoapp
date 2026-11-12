# Review — Task 17: Database Backup & Restore

**Date:** 2026-11-12 · **Branch:** task/17-database-backup · **Verdict:** ✅ PASS

## Summary
`TodoAppDb` is now backed up every day at 02:00 by the Windows Task Scheduler task "TodoAppDb daily backup" (SQL Server Express, so Option A). The backup uses `CHECKSUM`, is verified with `RESTORE VERIFYONLY`, and backups older than 7 days are removed only after a verified backup. Every run writes one log line. Two scheduled runs, the cleanup, the failure path and a test restore were all verified. `ops/RESTORE.md` documents restore through SSMS and through T-SQL. The client ran the one-time elevated setup (folder permission and task registration with their own password).

## Automated checks
| Check | Command | Result |
|---|---|---|
| Backend / frontend checks | — | N/A: no `backend/` or `frontend/` changes (`git diff main...HEAD --stat`: `.gitignore`, `ops/*`, `plans/*`) |
| PowerShell syntax | `[Parser]::ParseFile` on both scripts | ✅ 0 parse errors each |
| Installed copy = repo | `Get-FileHash` on `ops\backup-todoapp.ps1` and `C:\Backups\TodoAppDb\backup-todoapp.ps1` | ✅ identical |
| Secrets / password handling | `Select-String password` in `ops\*.ps1` | ✅ The backup uses Windows auth (`sqlcmd -E`). Setup reads the password only through `Get-Credential` and passes it straight to `Register-ScheduledTask`; never printed, stored or logged |
| Raw SQL | — | ✅ Only `BACKUP DATABASE` / `RESTORE VERIFYONLY` in `ops/backup-todoapp.ps1`, the exception allowed by G-SEC-4 |
| Tracked files | `git ls-files ops`, `git check-ignore -v ops/backup.log` | ✅ Only the 3 ops files tracked; `ops/backup.log` ignored (`.gitignore:19`); `*.bak` already ignored |
| Packages | — | ✅ none (built-in `sqlcmd`, PowerShell 5.1, ScheduledTasks module) |

## Definition of Done
| Item | Status | Evidence |
|---|---|---|
| Step: choose method by edition | ✅ | `SERVERPROPERTY('Edition')` = Express Edition (64-bit), SQL Agent Disabled → Option A. Option B marked N/A |
| Step: backup folder outside the repo | ✅ | `C:\Backups\TodoAppDb` (no D: drive on this machine) |
| Step: SQL service account write permission | ✅ | ACL: `NT SERVICE\MSSQL$SQLEXPRESS` Modify, ContainerInherit + ObjectInherit (set by the client's elevated setup run) |
| Step: `ops/backup-todoapp.ps1` (BACKUP … WITH INIT, CHECKSUM; VERIFYONLY; delete > 7 days; log line) | ✅ | Script reviewed; log `OK TodoAppDb_20261112_0841.bak 6.93 MB, verified; removed 1 backup(s) …` |
| Step: Task Scheduler task (daily fixed time, logged on or not, run if missed) | ✅ | `Get-ScheduledTask`: Daily trigger 02:00, LogonType Password, StartWhenAvailable True; NextRunTime 13-11-2026 02:00 |
| Step: `ops/RESTORE.md` (SSMS + `RESTORE … WITH REPLACE`) | ✅ | Sections 2 (SSMS), 3 (T-SQL with `SINGLE_USER`, `REPLACE`, `CHECKSUM`), 4 (test copy), 5 (after restore) |
| Step: test restore as `TodoAppDb_RestoreTest`, compare counts, drop | ✅ | Commands from RESTORE.md section 4: 874 pages restored; counts 21 / 45 / 3,024 / 10 / 1 identical in both databases; test copy dropped, `TodoAppDb` ONLINE |
| DoD: scheduled backup ran, verified `.bak` exists | ✅ | Task Scheduler runs 08:39:43 and 08:41:02, LastTaskResult 0; `TodoAppDb_20261112_0839.bak` and `_0841.bak` verified |
| DoD: old-file cleanup confirmed | ✅ | Dummy `TodoAppDb_20261102_0200.bak` (10 days old) removed; dummy 6-day-old `TodoAppDb_*.bak` and 10-day-old `OtherDb_*.bak` kept (both dummies removed by hand afterwards) |
| DoD: RESTORE.md + test restore date | ✅ | Recorded in `ops/RESTORE.md` and in the task Notes: 2026-11-12 |
| Extra: failure path | ✅ | `-Database NoSuchDb` → log `FAILED Msg 911 … does not exist`, exit 1, no files deleted |

## Guardrails
| ID | Status | Notes |
|---|---|---|
| G-PROC-1 | ✅ | Tasks 01–16 ✅ in the index |
| G-PROC-2 | ✅ | Client ran `/start-process` |
| G-PROC-3 | ✅ | This report |
| G-PROC-4 to 6 | ✅ | — |
| G-PROC-7 | ✅ | The elevated setup (folder permission, task with the client's password) is a step Claude must not do, so it was handed to the client; the client ran it |
| G-SCOPE-1 to 3 | ✅ | NFR-5, NFR-6 only. One small change from the task text: the scheduled task runs a copy of the script in the backup folder and logs next to it (`C:\Backups\TodoAppDb\backup.log`), so the schedule does not break when a git checkout or worktree moves. Manual runs from `ops/` still log to `ops/backup.log` |
| G-SCOPE-4 | ✅ | Only `.gitignore`, `ops/*` and plan files |
| G-GIT-1 | ✅ | `task/17-database-backup` from `main` at `ad88349` |
| G-GIT-2 | ✅ | All 5 commits start with `[Task 17]` |
| G-GIT-3, 4 | ✅ | Not merged; no remote |
| G-GIT-5 | ✅ | `ops/backup.log` and `*.bak` ignored; backups live outside the repo |
| G-DEP-1 to 3 | ✅ | No packages |
| G-SEC-1, 2 | ✅ | No secrets; password never logged |
| G-SEC-3 | N/A | — |
| G-SEC-4 | ✅ | Raw SQL only in the backup script (allowed exception) and in RESTORE.md instructions |
| G-SEC-5 to 8 | N/A | No app code changes |
| G-CODE-1 to 8 | N/A / ✅ | No C#/TS changes; scripts have no dead code or TODOs; retention days and paths are named parameters |
| G-TEST-1 to 4 | N/A | No app code changes; scripts verified by real runs (success, cleanup, failure) |
| G-DB-1 to 4 | ✅ | No schema changes; `TodoAppDb` itself was never restored over or modified |
| G-DESTRUCT-1 | ✅ | Only `TodoAppDb_RestoreTest` was dropped: created in this task and dropped as the task file requires. `TodoAppDb` untouched |
| G-DESTRUCT-2 to 4 | ✅ | Only files created in this task were deleted (the dummy `.bak` files) |

## Issues to fix (FAIL only)
None.

## Not verified
- **The 02:00 trigger itself** — the task ran twice from Task Scheduler (`Start-ScheduledTask`, same account and "run whether logged on or not" mode), but its first timed run is on 2026-11-13 02:00. Please check `C:\Backups\TodoAppDb\backup.log` tomorrow for a new `OK` line.
- **First setup run failed** — the 08:36 run exited with 1 because Windows PowerShell 5.1 leaves `$PSScriptRoot` empty in `param()` defaults; the script stopped before it could log. Fixed in commit `5643292`, the installed copy was replaced with the fixed version (hash checked), and later runs succeeded.
- **Windows password change** — the task stores the account password; after changing the Windows password, run `ops/setup-backup-schedule.ps1` again (or update the task in Task Scheduler).
- **Script changes** — the scheduled copy in `C:\Backups\TodoAppDb` is not updated by git; re-run the setup after changing `ops/backup-todoapp.ps1`.
- **Off-site copy** — backups are on the same disk as the database (not required by NFR-5).

## Files changed outside this task's scope
- `.gitignore` — ignore `ops/backup.log`.
- `plans/00-INDEX.md` — Task 17 status.

## Review history
| Run | Date | Verdict |
|---|---|---|
| 1 | 2026-11-12 | ✅ PASS |
