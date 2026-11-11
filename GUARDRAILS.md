# Development Guardrails — Todo Application

**Version:** 1.0
**Date:** 2026-11-11
**Applies to:** Every task in [plans/](plans/00-INDEX.md), for both the AI developer (Claude) and human developers.
**Related:** [BRD.md](BRD.md) · [ARCHITECTURE.md](ARCHITECTURE.md)

Every guardrail has an ID. The `/task-review` skill checks each one after every task. A single **FAIL** blocks the task.

---

## 1. Task Workflow (G-PROC)

```
Start task NN on branch task/NN-<name>
        │
        ▼
Implement steps + Definition of Done
        │
        ▼
Run /task-review NN  ──► FAIL ──► fix issues ──► run /task-review NN again
        │
       PASS
        │
        ▼
STOP: show the review report and ask the client to approve the merge
        │
   client says "yes"
        │
        ▼
Merge into main (local) ──► mark task ✅ in plans/00-INDEX.md ──► start task NN+1
```

| ID | Rule |
|---|---|
| G-PROC-1 | Tasks run **strictly in the order** of `plans/00-INDEX.md`. A task never starts before the previous one is merged. |
| G-PROC-2 | Implementation of the project starts only after the client says **"yes"**. The client running **`/start-process`** counts as that "yes". It never counts as merge approval. |
| G-PROC-3 | After finishing a task, run **`/task-review <NN>`**. The review report is saved as `reviews/task-NN-review.md`. |
| G-PROC-4 | If the review result is **FAIL**, do **not** move to the next task. Fix every failed item and re-run the review until it passes. |
| G-PROC-5 | If the review result is **PASS**, **stop** and ask the client for approval to merge. Merge only after the client's explicit "yes". |
| G-PROC-6 | After merging, mark the task ✅ Done in `plans/00-INDEX.md` before starting the next task. |
| G-PROC-7 | If a task is blocked (missing tool, unclear requirement, failing environment), stop and ask. Never guess around a blocker. |

## 2. Scope (G-SCOPE)

| ID | Rule |
|---|---|
| G-SCOPE-1 | Build **only** what is in the BRD, ARCHITECTURE and the current task file. |
| G-SCOPE-2 | Anything not written there (a new feature, field, endpoint, screen or behavior change) needs the **client's approval first**. After approval, update the BRD (and ARCHITECTURE if needed) **before** implementing it. |
| G-SCOPE-3 | Phase 2 backlog items (BRD §8) are never built in v1, even partially. |
| G-SCOPE-4 | A task only changes files that belong to that task. Changes to earlier tasks' code are allowed only when needed for the current task, and they are listed in the review report. |

## 3. Git (G-GIT)

| ID | Rule |
|---|---|
| G-GIT-1 | Every task gets its own branch from the latest `main`: `task/NN-<short-name>` (e.g. `task/04-auth-api`). |
| G-GIT-2 | Commit messages: `[Task NN] <imperative summary>` (e.g. `[Task 04] Add JWT login endpoint`). |
| G-GIT-3 | Merge into `main` only after the review **PASS** **and** the client's approval (G-PROC-5). |
| G-GIT-4 | **Never push** to any remote unless the client asks. |
| G-GIT-5 | Never commit `bin/`, `obj/`, `node_modules/`, `logs/`, `.bak` files, or user-secret files. |

## 4. Dependencies (G-DEP)

| ID | Rule |
|---|---|
| G-DEP-1 | Only packages on the **Approved Package List** (Appendix A) may be added. |
| G-DEP-2 | Any other package needs the client's approval with a reason (what it does, why the approved stack is not enough, licence). After approval, add it to Appendix A. |
| G-DEP-3 | Packages must be stable releases (no preview/beta) with an OSI-approved free licence. |

## 5. Security (G-SEC) — non-negotiable

| ID | Rule |
|---|---|
| G-SEC-1 | **No secrets in code or git.** Connection strings, JWT key and passwords live only in user secrets or environment variables. `appsettings*.json` contains no real secret values. |
| G-SEC-2 | **Never log sensitive data:** no passwords, JWT tokens, `Authorization` headers or full request/response bodies in logs. |
| G-SEC-3 | **`UserId` comes only from the JWT** (`ICurrentUserService`). It is never read from a request body, query string or route. |
| G-SEC-4 | **No raw SQL.** No `FromSqlRaw`, `ExecuteSqlRaw`, `SqlCommand` or string-built queries. Use EF Core LINQ only. (The backup script in Task 17 is the only exception.) |
| G-SEC-5 | **No `innerHTML`, `[innerHTML]`, `outerHTML` or `bypassSecurityTrust*`** in Angular. User content is shown only through interpolation (`{{ }}`). |
| G-SEC-6 | Every endpoint requires authentication, except `register` and `login` (`[AllowAnonymous]`). |
| G-SEC-7 | API errors never expose stack traces, SQL or internal exception messages (NFR-18). |
| G-SEC-8 | Accessing another user's resource returns **404**, never the data (AD-5). |

## 6. Code Quality (G-CODE)

| ID | Rule |
|---|---|
| G-CODE-1 | **Zero build warnings.** C#: `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in every project. Angular: `strict: true` in `tsconfig.json` and strict templates on. |
| G-CODE-2 | **Formatter + linter clean.** Backend: `.editorconfig` + `dotnet format --verify-no-changes`. Frontend: ESLint (`ng lint`) + Prettier (`prettier --check`). |
| G-CODE-3 | **Layering:** no business logic or `DbContext` use in controllers; controllers call services only. |
| G-CODE-4 | **Entities are never returned from the API.** Always map to DTOs. |
| G-CODE-5 | **Naming:** async methods end with `Async`; interfaces start with `I`; C# PascalCase for types/methods, camelCase for locals; Angular files use kebab-case (`task-list.component.ts`). |
| G-CODE-6 | **One public class/interface per file**, file name = type name. |
| G-CODE-7 | **No dead code:** no commented-out code, no unused usings/imports/variables, no `console.log`, no `TODO`/`FIXME` left in merged code. |
| G-CODE-8 | No magic numbers for business rules (lockout count, token expiry, rate limits, "due soon" hours). Use configuration or named constants. |

## 7. Testing (G-TEST)

| ID | Rule |
|---|---|
| G-TEST-1 | **All tests pass.** No failing tests, no `[Fact(Skip = ...)]`, no `xit`/`fdescribe`. |
| G-TEST-2 | **Every new public service method has tests:** at least the happy path **and** one failure/not-found path. |
| G-TEST-3 | Services that touch user data have an **ownership test** (user B cannot access user A's data). |
| G-TEST-4 | Service-layer coverage stays **≥ 60%** (NFR-26) from Task 09 onward. |

## 8. Database (G-DB)

| ID | Rule |
|---|---|
| G-DB-1 | Schema changes only through **new EF Core migrations**. Never edit or delete a migration that has already been applied. |
| G-DB-2 | Review every generated migration before applying it. |
| G-DB-3 | Deletes are **soft deletes** (NFR-7). No `RemoveRange`/hard delete of user data. |
| G-DB-4 | All `DateTime` values are stored in **UTC**. |

## 9. Destructive Actions (G-DESTRUCT)

Ask the client and wait for an explicit "yes" **before** any of these:

| ID | Action |
|---|---|
| G-DESTRUCT-1 | Dropping, resetting or re-creating the database (`dotnet ef database drop`, `DROP DATABASE`, restoring over `TodoAppDb`). |
| G-DESTRUCT-2 | `git reset --hard`, `git push --force`, deleting branches, rewriting history. |
| G-DESTRUCT-3 | Deleting files or folders that were not created in the current task. |
| G-DESTRUCT-4 | Removing or downgrading an installed package or SDK. |

---

## Appendix A — Approved Package List

### Backend (`TodoApp.Api`)
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `FluentValidation.AspNetCore` / `FluentValidation.DependencyInjectionExtensions`
- `Serilog.AspNetCore`, `Serilog.Sinks.File`
- `Swashbuckle.AspNetCore`
- `System.Security.Cryptography.Pkcs` (security pin only: overrides the vulnerable transitive 9.0.11 from `Microsoft.Data.SqlClient`)

### Backend tests (`TodoApp.Tests`)
- `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` (template defaults)
- `FluentAssertions`
- `Microsoft.EntityFrameworkCore.InMemory` or `Microsoft.EntityFrameworkCore.Sqlite`
- `Moq`
- `coverlet.collector`

### .NET tools
- `dotnet-ef`, `dotnet-reportgenerator-globaltool`

### Frontend (`todo-app`)
- Everything generated by `ng new` (`@angular/*`, `rxjs`, `tslib`, `zone.js` if generated, TypeScript and test tooling)
- `@angular/material` + `@angular/cdk` (only if Material is chosen in Task 10)
- `angular-eslint` (via `ng add angular-eslint`)
- `prettier`, `eslint-config-prettier`

*New entries are added here only after client approval (G-DEP-2).*

## Appendix B — Change Log

| Date | Change | Approved by |
|---|---|---|
| 2026-11-11 | Initial guardrails | Client |
| 2026-11-11 | Added `System.Security.Cryptography.Pkcs` 10.0.14 (MIT) to Appendix A as a direct pin. EF Core SqlServer 10.0.11 pulls 9.0.11 via `Microsoft.Data.SqlClient`, which has advisories GHSA-846f-f764-76mx (high) and GHSA-2fmv-rv2c-59x7 (moderate) and fails the build under warnings-as-errors (Task 02) | Client |
