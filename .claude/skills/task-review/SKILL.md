---
name: task-review
description: Review a finished implementation task (plans/NN-*.md) of the Todo Application against its Definition of Done, GUARDRAILS.md, BRD and ARCHITECTURE, and write a PASS/FAIL report to reviews/task-NN-review.md. Use after every task, before asking the client to approve the merge. Usage - /task-review <NN> (e.g. /task-review 04).
---

# Task Review

You are the **reviewer**, not the implementer. Check the work honestly and strictly. **Do not fix anything, merge, or start the next task inside this skill.** Only inspect, run read-only checks, and write the report.

## 0. Inputs

1. Task number `NN` comes from the arguments. If it is missing, take it from the current git branch name `task/NN-...`. If it is still unknown, ask.
2. Read, in full:
   - `plans/NN-*.md` (the task file)
   - `GUARDRAILS.md`
   - the BRD sections and ARCHITECTURE sections the task file lists under **References**
3. Get the change set: `git diff --stat main...HEAD` and `git diff main...HEAD`, plus `git status` for uncommitted files. Every changed file is in scope for the review.

## 1. Automated checks

Run only the checks that apply to the code that exists so far (backend checks once `backend/` exists, frontend checks once `frontend/todo-app/` exists). Record the exact command and the result (pass/fail + key output lines).

### Backend (from `backend/`)
| Check | Command | Guardrail |
|---|---|---|
| Build, zero warnings | `dotnet build --no-incremental` (must show `0 Warning(s)` and `0 Error(s)`) | G-CODE-1 |
| Nullable + warnings-as-errors set | inspect every `*.csproj` for `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` | G-CODE-1 |
| Format | `dotnet format --verify-no-changes` | G-CODE-2 |
| Tests | `dotnet test` (0 failed, 0 skipped) | G-TEST-1 |
| Coverage (Task 09 onward) | `dotnet test --collect:"XPlat Code Coverage"` + reportgenerator; Services ≥ 60% | G-TEST-4 |

### Frontend (from `frontend/todo-app/`)
| Check | Command | Guardrail |
|---|---|---|
| Lint | `npx ng lint` | G-CODE-2 |
| Prettier | `npx prettier --check "src/**/*.{ts,html,scss}"` | G-CODE-2 |
| Strict mode | inspect `tsconfig.json` for `"strict": true` and `strictTemplates` | G-CODE-1 |
| Build | `npx ng build` (no errors, no warnings except approved budget notes) | G-CODE-1 |

### Code searches (on changed files; use Grep)
| Search for | Guardrail |
|---|---|
| Secrets: `Password=`, `Pwd=`, `User Id=`, `"Key":` with a real value, long random strings in `appsettings*.json`, `.cs`, `.ts` | G-SEC-1 |
| Logging of `password`, `token`, `Authorization`, or whole request/response objects in `_logger.`/`Log.` calls | G-SEC-2 |
| `UserId` read from DTOs, `[FromBody]`, `[FromQuery]` or `[FromRoute]` | G-SEC-3 |
| `FromSqlRaw`, `FromSqlInterpolated`, `ExecuteSqlRaw`, `ExecuteSqlInterpolated`, `SqlCommand`, `SqlConnection` | G-SEC-4 |
| `innerHTML`, `outerHTML`, `bypassSecurityTrust` | G-SEC-5 |
| `[AllowAnonymous]` anywhere except register/login (and an optional health endpoint) | G-SEC-6 |
| `DbContext`/`AppDbContext` used inside `Controllers/` | G-CODE-3 |
| Entity types (`TodoTask`, `Category`, `SubTask`, `AppUser`) as controller return types | G-CODE-4 |
| `async` methods without the `Async` suffix; files with more than one public type | G-CODE-5, G-CODE-6 |
| `TODO`, `FIXME`, `console.log`, blocks of commented-out code | G-CODE-7 |
| Hard-coded `5`, `15`, `60`, `24`, `10`, `100` used as business rules | G-CODE-8 |
| `Skip =`, `xit(`, `fit(`, `fdescribe(`, `xdescribe(` | G-TEST-1 |
| `RemoveRange`, `.Remove(` on user data without soft delete | G-DB-3 |
| `DateTime.Now` (should be `DateTime.UtcNow`) | G-DB-4 |

### Git and dependency checks
| Check | Guardrail |
|---|---|
| Current branch is `task/NN-<name>`, created from `main` | G-GIT-1 |
| Every commit message matches `[Task NN] ...` (`git log main..HEAD --oneline`) | G-GIT-2 |
| Nothing pushed (`git status` / no upstream changes) | G-GIT-4 |
| No `bin/`, `obj/`, `node_modules/`, `logs/`, `.bak` or secret files tracked | G-GIT-5 |
| Every package added in `*.csproj` / `package.json` diff is on GUARDRAILS Appendix A | G-DEP-1, G-DEP-3 |
| No previously committed file under `Migrations/` was modified or deleted (`git diff main...HEAD --name-status -- '**/Migrations/**'` must show only `A` for new files) | G-DB-1 |

## 2. Manual review

1. **Definition of Done:** go through every step and every DoD item in the task file. For each, mark ✅ met (with evidence: a file, test name or command output) or ❌ not met.
2. **Scope (G-SCOPE-1 to 4):** compare the diff with the task file, BRD and ARCHITECTURE. Flag any feature, field, endpoint, screen or behavior that is not specified there, any Phase 2 item, and any change to another task's files that the task did not need.
3. **Tests (G-TEST-2, G-TEST-3):** for every new public service method, confirm there is a happy-path test and a failure/not-found test; for user-data services, confirm an ownership test exists.
4. **Security (G-SEC-6 to 8):** confirm that error responses use ProblemDetails without internals, and that another user's resource returns 404.
5. **Destructive actions (G-DESTRUCT):** check whether any database drop/reset, history rewrite or deletion of other files happened in this task, and whether client approval for it is recorded.
6. **Process (G-PROC-1):** confirm all earlier tasks are ✅ in `plans/00-INDEX.md`.

Items that cannot be verified in this environment (e.g. a check that needs a browser) are marked **⚠️ NOT VERIFIED** with the reason. They are not counted as PASS.

## 3. Verdict

- **FAIL** if any automated check fails, any DoD item is ❌, or any guardrail is violated.
- **PASS** only if everything is ✅. ⚠️ NOT VERIFIED items are listed so the client can decide; they do not silently pass.

## 4. Report

Write `reviews/task-NN-review.md` (overwrite the previous run, but keep a "Review history" table at the bottom with each run's date and verdict):

```markdown
# Review — Task NN: <task name>

**Date:** YYYY-MM-DD · **Branch:** task/NN-... · **Verdict:** ✅ PASS / ❌ FAIL

## Summary
<2–3 sentences>

## Automated checks
| Check | Command | Result |
|---|---|---|

## Definition of Done
| Item | Status | Evidence |
|---|---|---|

## Guardrails
| ID | Status | Notes |
|---|---|---|
(list every guardrail ID from GUARDRAILS.md; mark N/A where it does not apply to this task yet)

## Issues to fix (FAIL only)
1. <file:line> — <what is wrong> — <guardrail / DoD item>

## Not verified
- <item> — <reason>

## Files changed outside this task's scope
- <file> — <why it was needed>

## Review history
| Run | Date | Verdict |
|---|---|---|
```

## 5. After the report

- **FAIL:** tell the user the verdict, list the issues, and say the next step is to fix them and re-run `/task-review NN`. Do not start the next task.
- **PASS:** tell the user the verdict with a short summary and the report path, then **ask for approval to merge `task/NN-...` into `main`** (G-PROC-5). Do not merge until the user says yes.
