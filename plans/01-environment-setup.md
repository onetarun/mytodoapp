# Task 01 — Environment Setup

**Depends on:** —
**References:** BRD §3, §9.1 · ARCHITECTURE AD-2
**Goal:** Every tool needed to build and run the app is installed and verified on the development machine.

## Steps

- [x] Install the **.NET SDK (latest LTS)** and verify with `dotnet --version`.
- [x] Install **SQL Server** (Express or Developer edition) and note the instance name (e.g. `localhost\SQLEXPRESS`).
- [x] Install **SQL Server Management Studio (SSMS)** or Azure Data Studio, and connect to the instance.
- [x] Note which edition is installed: **Express (no SQL Agent)** or **Developer (has SQL Agent)**. This decides the backup approach in Task 17.
- [x] Install the EF Core CLI tool: `dotnet tool install --global dotnet-ef`, then verify with `dotnet ef --version`.
- [x] Install **Node.js (LTS)** and verify with `node -v` and `npm -v`.
- [x] Install the **Angular CLI**: `npm install -g @angular/cli`, then verify with `ng version`.
- [x] Trust the .NET HTTPS development certificate: `dotnet dev-certs https --trust`.
- [x] Install an editor (VS Code with C# Dev Kit + Angular Language Service, or Visual Studio).
- [x] Confirm git is initialized with a `.gitignore` for .NET and Node. (`/start-process` does this on its first run, before this task's branch is created.)

## Definition of Done

- [x] `dotnet --version`, `dotnet ef --version`, `node -v` and `ng version` all print versions.
- [x] SSMS / Azure Data Studio connects to the local SQL Server instance.
- [x] HTTPS dev certificate is trusted.
- [x] SQL Server edition and instance name are recorded in this file (below).

## Notes

- SQL Server instance name: `localhost\SQLEXPRESS` (server name `DESKTOP-2L8T6C7\SQLEXPRESS`)
- Edition: **Express** (Express Edition 64-bit, version 16.0.1000.6 = SQL Server 2022). No usable SQL Agent jobs, so Task 17 uses the Express backup approach.

### Verification (2026-11-11)

| Tool | Command | Result |
|---|---|---|
| .NET SDK | `dotnet --version` | `10.0.403` (.NET 10 LTS) |
| EF Core CLI | `dotnet ef --version` | `10.0.11` |
| Node.js / npm | `node -v` / `npm -v` | `v24.18.0` / `11.16.0` |
| Angular CLI | `ng version` | `22.0.7` |
| HTTPS dev cert | `dotnet dev-certs https --check --trust` | trusted certificate found (exit code 0) |
| SQL Server | `Get-Service MSSQL$SQLEXPRESS` | `Running` |
| SQL connection | `sqlcmd -S "localhost\SQLEXPRESS" -E -C -Q "SELECT SERVERPROPERTY('Edition')"` | connected with Windows auth; `Express Edition (64-bit)` |
| SSMS | install folder check | `SQL Server Management Studio 21` installed |
| Editor | `vswhere -products *` / `code --list-extensions` | Visual Studio Community 2026; VS Code with Angular Language Service (`angular.ng-template`) |
| git | `git --version` | `2.55.0.windows.3`; repo initialized on `main` with `.gitignore` |

The SQL Server connection was verified with `sqlcmd` (Windows authentication). Opening the connection in the SSMS GUI was not done by the AI developer; the client should confirm it once.
