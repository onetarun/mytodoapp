# Task 01 — Environment Setup

**Depends on:** —
**References:** BRD §3, §9.1 · ARCHITECTURE AD-2
**Goal:** Every tool needed to build and run the app is installed and verified on the development machine.

## Steps

- [ ] Install the **.NET SDK (latest LTS)** and verify with `dotnet --version`.
- [ ] Install **SQL Server** (Express or Developer edition) and note the instance name (e.g. `localhost\SQLEXPRESS`).
- [ ] Install **SQL Server Management Studio (SSMS)** or Azure Data Studio, and connect to the instance.
- [ ] Note which edition is installed: **Express (no SQL Agent)** or **Developer (has SQL Agent)**. This decides the backup approach in Task 17.
- [ ] Install the EF Core CLI tool: `dotnet tool install --global dotnet-ef`, then verify with `dotnet ef --version`.
- [ ] Install **Node.js (LTS)** and verify with `node -v` and `npm -v`.
- [ ] Install the **Angular CLI**: `npm install -g @angular/cli`, then verify with `ng version`.
- [ ] Trust the .NET HTTPS development certificate: `dotnet dev-certs https --trust`.
- [ ] Install an editor (VS Code with C# Dev Kit + Angular Language Service, or Visual Studio).
- [ ] Confirm git is initialized with a `.gitignore` for .NET and Node. (`/start-process` does this on its first run, before this task's branch is created.)

## Definition of Done

- [ ] `dotnet --version`, `dotnet ef --version`, `node -v` and `ng version` all print versions.
- [ ] SSMS / Azure Data Studio connects to the local SQL Server instance.
- [ ] HTTPS dev certificate is trusted.
- [ ] SQL Server edition and instance name are recorded in this file (below).

## Notes

- SQL Server instance name: `________________`
- Edition: `Express / Developer` (circle one)
