# Task 17 — Database Backup & Restore

**Depends on:** 16
**References:** NFR-5, NFR-6 · ARCHITECTURE §5 (Backup), §12
**Goal:** `TodoAppDb` is backed up automatically every day, 7 days are kept, and a restore has been tested once.

## Steps

### Choose the method (from the edition noted in Task 01)
- [x] **Express edition** → Option A (Task Scheduler). *(`SERVERPROPERTY('Edition')` = Express Edition (64-bit); SQL Agent service is Disabled.)*
- N/A — **Developer / Standard edition** → Option B (SQL Server Agent). *(Not this machine.)*

### Option A — Windows Task Scheduler + `sqlcmd`
- [x] Create a backup folder, e.g. `D:\Backups\TodoAppDb\` (outside the project and the git repo). *(`C:\Backups\TodoAppDb` — the machine has only drive C:.)*
- [x] Give the SQL Server service account write permission on that folder. *(`NT SERVICE\MSSQL$SQLEXPRESS`: Modify, inherited; done by the client with `ops/setup-backup-schedule.ps1` in an elevated PowerShell.)*
- [x] Write `ops/backup-todoapp.ps1` that:
  - [x] runs `sqlcmd -S <instance> -E -Q "BACKUP DATABASE [TodoAppDb] TO DISK = N'<folder>\TodoAppDb_<yyyyMMdd_HHmm>.bak' WITH INIT, CHECKSUM"`;
  - [x] verifies the file with `RESTORE VERIFYONLY`;
  - [x] deletes `.bak` files **older than 7 days**; *(only `TodoAppDb_*.bak` in the backup folder, and only after a verified backup)*
  - [x] writes a line to `ops/backup.log` (success or failure). *(Log next to the script: `ops/backup.log` for manual runs, `C:\Backups\TodoAppDbbackup.log` for the scheduled copy — see Notes.)*
- [x] Create a Task Scheduler task: daily at a fixed time (e.g. 02:00), "run whether user is logged on or not", run if missed. *("TodoAppDb daily backup": daily 02:00, LogonType Password, StartWhenAvailable True, 1 h limit; registered by the client with `ops/setup-backup-schedule.ps1`.)*

### Option B — SQL Server Agent
- N/A — Create a Maintenance Plan or Agent job with a daily full backup of `TodoAppDb` to the backup folder, `CHECKSUM` on.
- N/A — Add a cleanup step that removes backups older than 7 days.
- N/A — Make sure the SQL Server Agent service is set to **Automatic** start.

### Restore (NFR-6)
- [x] Write `ops/RESTORE.md` with step-by-step restore instructions (SSMS and the `RESTORE DATABASE ... WITH REPLACE` command).
- [x] **Test once:** restore the latest backup as `TodoAppDb_RestoreTest`, check row counts in the tables match, then drop the test database.

## Definition of Done

- [x] The scheduled backup has run at least once and a verified `.bak` file exists. *(Task Scheduler runs at 08:39:43 and 08:41:04, LastTaskResult 0; log `OK TodoAppDb_20261112_0839.bak 6.93 MB, verified`.)*
- [x] Old-file cleanup is confirmed (e.g. a dummy file with an old date was removed). *(Dummy `TodoAppDb_20261102_0200.bak` dated 10 days back was removed; a 6-day-old one and an `OtherDb_*.bak` were kept. A failed backup (`-Database NoSuchDb`) logs `FAILED`, exits 1 and deletes nothing.)*
- [x] `ops/RESTORE.md` exists and the test restore succeeded (record the date below). *(2026-11-12: restored as `TodoAppDb_RestoreTest`, row counts match, copy dropped.)*

## Notes

- Method used: `Task Scheduler` (SQL Server Express) — task "TodoAppDb daily backup", daily 02:00, runs `C:\Backups\TodoAppDbbackup-todoapp.ps1` (a copy installed by `ops/setup-backup-schedule.ps1`, so the schedule does not depend on a git checkout; re-run the setup after changing the script). Its log is `C:\Backups\TodoAppDbbackup.log`.
- Backup folder: `C:\Backups\TodoAppDb`
- Test restore done on: `2026-11-12`
