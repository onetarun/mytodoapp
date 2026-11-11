# Task 17 — Database Backup & Restore

**Depends on:** 16
**References:** NFR-5, NFR-6 · ARCHITECTURE §5 (Backup), §12
**Goal:** `TodoAppDb` is backed up automatically every day, 7 days are kept, and a restore has been tested once.

## Steps

### Choose the method (from the edition noted in Task 01)
- [ ] **Express edition** → Option A (Task Scheduler).
- [ ] **Developer / Standard edition** → Option B (SQL Server Agent). Option A also works here.

### Option A — Windows Task Scheduler + `sqlcmd`
- [ ] Create a backup folder, e.g. `D:\Backups\TodoAppDb\` (outside the project and the git repo).
- [ ] Give the SQL Server service account write permission on that folder.
- [ ] Write `ops/backup-todoapp.ps1` that:
  - [ ] runs `sqlcmd -S <instance> -E -Q "BACKUP DATABASE [TodoAppDb] TO DISK = N'<folder>\TodoAppDb_<yyyyMMdd_HHmm>.bak' WITH INIT, CHECKSUM"`;
  - [ ] verifies the file with `RESTORE VERIFYONLY`;
  - [ ] deletes `.bak` files **older than 7 days**;
  - [ ] writes a line to `ops/backup.log` (success or failure).
- [ ] Create a Task Scheduler task: daily at a fixed time (e.g. 02:00), "run whether user is logged on or not", run if missed.

### Option B — SQL Server Agent
- [ ] Create a Maintenance Plan or Agent job with a daily full backup of `TodoAppDb` to the backup folder, `CHECKSUM` on.
- [ ] Add a cleanup step that removes backups older than 7 days.
- [ ] Make sure the SQL Server Agent service is set to **Automatic** start.

### Restore (NFR-6)
- [ ] Write `ops/RESTORE.md` with step-by-step restore instructions (SSMS and the `RESTORE DATABASE ... WITH REPLACE` command).
- [ ] **Test once:** restore the latest backup as `TodoAppDb_RestoreTest`, check row counts in the tables match, then drop the test database.

## Definition of Done

- [ ] The scheduled backup has run at least once and a verified `.bak` file exists.
- [ ] Old-file cleanup is confirmed (e.g. a dummy file with an old date was removed).
- [ ] `ops/RESTORE.md` exists and the test restore succeeded (record the date below).

## Notes

- Method used: `Task Scheduler / SQL Agent`
- Backup folder: `________________`
- Test restore done on: `____-__-__`
