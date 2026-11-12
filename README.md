# Todo Application

A simple, fast task tracker for individual users: tasks with due dates, priorities, categories and checklists, a dashboard, and an in-app notification bell.

- **Backend:** ASP.NET Core Web API (.NET 10), EF Core + SQL Server, ASP.NET Core Identity + JWT — `backend/`
- **Frontend:** Angular 22 + Angular Material — `frontend/todo-app/`
- **Docs:** [BRD.md](BRD.md) (requirements) · [ARCHITECTURE.md](ARCHITECTURE.md) · [GUARDRAILS.md](GUARDRAILS.md) · [plans/](plans/00-INDEX.md) · [ops/RESTORE.md](ops/RESTORE.md)

---

## 1. Prerequisites

| Tool | Version used | Check |
|---|---|---|
| .NET SDK | 10.0.x (LTS) | `dotnet --version` |
| EF Core CLI | 10.0.x | `dotnet ef --version` (install: `dotnet tool install --global dotnet-ef`) |
| Node.js / npm | 24.x / 11.x | `node -v`, `npm -v` |
| SQL Server | 2022 Express or Developer, Windows authentication | `sqlcmd -S "localhost\SQLEXPRESS" -E -C -Q "SELECT @@VERSION"` |
| HTTPS dev certificate | — | `dotnet dev-certs https --trust` (once per machine) |
| Browser | latest Chrome or Edge (NFR-25) | — |

The Angular CLI does not need a global install; the commands below use the project's own copy through `npm` / `npx`.

## 2. Configure secrets (once per machine)

Secrets are **never** stored in `appsettings*.json` or in git (GUARDRAILS G-SEC-1). The API reads them from .NET user secrets:

```bash
cd backend/TodoApp.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=TodoAppDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<a random string of at least 32 characters>"
dotnet user-secrets list
```

Generate a key in PowerShell, for example:

```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

The API refuses to start if `Jwt:Key` is missing or shorter than 32 bytes. On a server, use environment variables instead (`ConnectionStrings__DefaultConnection`, `Jwt__Key`).

## 3. Create the database

```bash
cd backend/TodoApp.Api
dotnet ef database update
```

This creates `TodoAppDb` with all tables and indexes from `Data/Migrations`. Run it again after pulling new migrations.

## 4. Run the app

Two terminals:

```bash
# Terminal 1 — API on https://localhost:7146 (Swagger: https://localhost:7146/swagger)
cd backend/TodoApp.Api
dotnet run --launch-profile https
```

```bash
# Terminal 2 — Angular on http://localhost:4200 (calls to /api are proxied to the API)
cd frontend/todo-app
npm ci
npm start
```

Open http://localhost:4200, choose **Create an account**, and log in. New users get the categories **Work** and **Personal**.

## 5. Tests and quality checks

```bash
# Backend: build (warnings are errors), format, unit tests
cd backend
dotnet build
dotnet format --verify-no-changes
dotnet test
```

```bash
# Coverage (NFR-26: services ≥ 60 %)
cd backend
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
```

```bash
# Frontend: lint, format, production build
cd frontend/todo-app
npm run lint
npm run format:check
npm run build
```

## 6. Configuration

| Setting | Where | Default |
|---|---|---|
| JWT lifetime | `Jwt:ExpiryMinutes` in `appsettings.json` | 60 minutes (NFR-10) |
| Allowed web origin (CORS) | `Cors:AllowedOrigins` | `http://localhost:4200` (NFR-14) |
| Rate limits | `RateLimiting:LoginPermitLimit` / `DefaultPermitLimit` / `WindowSeconds` | 10 logins/min per IP, 100 requests/min per user (NFR-15) |
| Lockout | code constants (`IdentityPolicy`) | 5 failed logins → 15 minutes (NFR-11) |
| Logs | `Serilog` section | `backend/TodoApp.Api/logs/todoapp-<date>.log`, 30 days kept (NFR-17) |

## 7. Backup and restore

- Daily full backup at 02:00 by the Windows Task Scheduler task **"TodoAppDb daily backup"** to `C:\Backups\TodoAppDb`, 7 days kept (NFR-5).
- One-time setup (elevated PowerShell): `ops\setup-backup-schedule.ps1 -RunNow`. Re-run it after changing `ops/backup-todoapp.ps1` or the Windows password.
- Restore steps: [ops/RESTORE.md](ops/RESTORE.md) (NFR-6).

## 8. Project layout

```
backend/
  TodoApp.Api/      Controllers → Services → Data (EF Core), DTOs, Validators, Middleware
  TodoApp.Tests/    xUnit tests (services, validators, middleware)
frontend/todo-app/  Angular app: core/ (auth, http, layout), features/ (auth, dashboard, tasks, categories, notifications), shared/
ops/                Backup script, schedule setup, restore guide
plans/              Task plan (00-INDEX.md) · reviews/ — review reports per task
```
