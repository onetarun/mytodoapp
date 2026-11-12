# Restore TodoAppDb from a backup (NFR-6)

**Applies to:** SQL Server Express instance `localhost\SQLEXPRESS`, database `TodoAppDb`
**Backups:** `C:\Backups\TodoAppDb\TodoAppDb_<yyyyMMdd_HHmm>.bak` (daily full backup with `CHECKSUM`, last 7 days kept)
**Backup log:** `C:\Backups\TodoAppDb\backup.log` (scheduled runs) and `ops/backup.log` (manual runs)

> ⚠️ Restoring over `TodoAppDb` **replaces all current data** with the backup. Everything written after the
> backup time is lost. Get the client's approval first (GUARDRAILS G-DESTRUCT-1). When in doubt, restore as a
> **test copy** first (section 4) and compare.

---

## 1. Before you start

1. Pick the backup. The newest file is normally the right one:
   ```powershell
   Get-ChildItem C:\Backups\TodoAppDb\TodoAppDb_*.bak | Sort-Object LastWriteTime -Descending | Select-Object -First 7 Name, LastWriteTime, Length
   ```
2. Check that the last log lines say `OK ... verified`:
   ```powershell
   Get-Content C:\Backups\TodoAppDb\backup.log -Tail 5
   ```
3. Verify the chosen file (reads the whole file and checks the checksums; changes nothing):
   ```powershell
   sqlcmd -S .\SQLEXPRESS -E -C -Q "RESTORE VERIFYONLY FROM DISK = N'C:\Backups\TodoAppDb\TodoAppDb_20261112_0200.bak' WITH CHECKSUM"
   ```
   Expected: `The backup set on file 1 is valid.`
4. **Stop the API** (`TodoApp.Api`) so nothing writes to the database during the restore.

## 2. Restore with SQL Server Management Studio (SSMS)

1. Connect to `localhost\SQLEXPRESS` with Windows authentication.
2. In **Object Explorer**, right-click **Databases** → **Restore Database…**
3. **Source:** choose **Device** → **…** → **Add** → select the `.bak` file → **OK**.
4. **Destination → Database:** `TodoAppDb`.
5. Page **Options**:
   - tick **Overwrite the existing database (WITH REPLACE)**;
   - tick **Close existing connections to destination database**;
   - leave **Take tail-log backup before restore** unticked (the database uses full backups only).
6. **OK**. Wait for "Database 'TodoAppDb' restored successfully".
7. Continue with **section 5 (After the restore)**.

## 3. Restore with T-SQL (`sqlcmd`)

Run in PowerShell (replace the file name):

```powershell
sqlcmd -S .\SQLEXPRESS -E -C -b -Q "
ALTER DATABASE [TodoAppDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [TodoAppDb]
    FROM DISK = N'C:\Backups\TodoAppDb\TodoAppDb_20261112_0200.bak'
    WITH REPLACE, CHECKSUM, RECOVERY, STATS = 25;
ALTER DATABASE [TodoAppDb] SET MULTI_USER;
"
```

- `SINGLE_USER WITH ROLLBACK IMMEDIATE` closes open connections (the API should already be stopped).
- `REPLACE` overwrites the existing database; `CHECKSUM` re-checks the backup while restoring.
- If the restore fails, the database may stay in single-user mode: run
  `sqlcmd -S .\SQLEXPRESS -E -C -Q "ALTER DATABASE [TodoAppDb] SET MULTI_USER"`.

**Restoring on another machine / other data folder:** add `MOVE` for the two logical files
(names from `RESTORE FILELISTONLY FROM DISK = N'<file>'`):

```sql
WITH REPLACE, CHECKSUM, RECOVERY,
     MOVE N'TodoAppDb'     TO N'<data folder>\TodoAppDb.mdf',
     MOVE N'TodoAppDb_log' TO N'<data folder>\TodoAppDb_log.ldf'
```

## 4. Test restore as a copy (no risk to TodoAppDb)

Use this to prove a backup works (NFR-6), or to look at old data without touching the live database.

```powershell
$bak  = (Get-ChildItem C:\Backups\TodoAppDb\TodoAppDb_*.bak | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
$data = 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA'
sqlcmd -S .\SQLEXPRESS -E -C -b -Q "
RESTORE DATABASE [TodoAppDb_RestoreTest] FROM DISK = N'$bak'
WITH CHECKSUM, RECOVERY,
     MOVE N'TodoAppDb'     TO N'$data\TodoAppDb_RestoreTest.mdf',
     MOVE N'TodoAppDb_log' TO N'$data\TodoAppDb_RestoreTest_log.ldf';"
```

Compare row counts (soft-deleted rows included) between the copy and the live database. They match when no one
has written to `TodoAppDb` since the backup:

```powershell
foreach ($db in 'TodoAppDb', 'TodoAppDb_RestoreTest') {
  sqlcmd -S .\SQLEXPRESS -E -C -d $db -h -1 -W -Q "SET NOCOUNT ON;
    SELECT '$db', (SELECT COUNT(*) FROM AspNetUsers), (SELECT COUNT(*) FROM Categories),
           (SELECT COUNT(*) FROM TodoTasks), (SELECT COUNT(*) FROM SubTasks),
           (SELECT COUNT(*) FROM __EFMigrationsHistory);"
}
```

Remove the test copy afterwards (only the copy, **never** `TodoAppDb`):

```powershell
sqlcmd -S .\SQLEXPRESS -E -C -Q "DROP DATABASE [TodoAppDb_RestoreTest]"
```

## 5. After the restore

1. Check the tables and the migration history:
   ```powershell
   sqlcmd -S .\SQLEXPRESS -E -C -d TodoAppDb -Q "SELECT COUNT(*) AS Users FROM AspNetUsers; SELECT COUNT(*) AS Tasks FROM TodoTasks; SELECT TOP 1 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC"
   ```
   The latest `MigrationId` must match the newest file in `backend/TodoApp.Api/Migrations/`. If the backup is
   older than a migration, apply it with `dotnet ef database update` (from `backend/TodoApp.Api`).
2. Start the API and log in with a known account. Check the dashboard and the task list.
3. Note the restore (date, backup file, reason) in the project notes.

## Test restore record

| Date | Backup file | Result |
|---|---|---|
| 2026-11-12 | `TodoAppDb_20261112_0841.bak` (made by the scheduled task) | ✅ Restored as `TodoAppDb_RestoreTest` with `CHECKSUM` (874 pages, 0.036 s). Row counts equal to `TodoAppDb`: AspNetUsers 21, Categories 45, TodoTasks 3,024, SubTasks 10, __EFMigrationsHistory 1. Test copy dropped afterwards. |
