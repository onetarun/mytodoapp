# Task 09 — Backend Test Coverage Gate

**Depends on:** 08
**References:** NFR-26, NFR-27 · ARCHITECTURE §11
**Goal:** Confirm the backend is complete and stable before frontend work starts: ≥ 60% coverage on services and every endpoint documented in Swagger.

## Steps

- [x] Run `dotnet test --collect:"XPlat Code Coverage"`.
- [x] Generate a readable report (e.g. `reportgenerator`) and check coverage of the `Services/` namespace.
- [x] If coverage is below **60%**, add tests for the uncovered branches (validation failures, not-found paths, edge cases).
- [x] Review Swagger: every endpoint from ARCHITECTURE §6 is listed with request/response schemas and the Bearer lock icon (except register/login).
- [x] Add XML comments or `[ProducesResponseType]` attributes so status codes show in Swagger.
- [x] Do one manual end-to-end pass in Swagger: register → login → create category → create task → add subtasks → complete → dashboard → notifications → delete category → delete task.
- [x] Check `logs/` has no unexpected errors from the manual pass.
- [x] Commit the backend as a stable baseline.

## Definition of Done

- [x] All backend tests pass.
- [x] Service-layer coverage ≥ 60% (record the number below).
- [x] All 18 endpoints are documented in Swagger.
- [x] Manual Swagger pass completed with no errors.

## Notes

- Coverage achieved: **100 % lines (401/401), 92.4 % branches (85/92)** in `TodoApp.Api.Services` (2026-11-11).
- Coverage was read straight from the `coverlet` Cobertura file (`TestResults/*/coverage.cobertura.xml`) with a small script that sums line and branch hits per namespace and per class. `dotnet-reportgenerator-globaltool` is on Appendix A but is **not installed** on this machine. It was not installed, because installing tools is the client's call; it can be added later for an HTML report.
- Other namespaces, for reference: Controllers 100 %, Middleware 100 %, Data 91.8 %, Validators 69.5 % (`ValidationFilter` runs only in the HTTP pipeline; checked through the real API), Extensions 35.1 % (startup wiring). Migrations and `Program.cs` are not unit tested.
- Swagger: the global security requirement was replaced by `Extensions/BearerSecurityOperationFilter`. It adds the Bearer lock and a `401` response only to endpoints without `[AllowAnonymous]`, so register, login and health have no lock. The status codes come from the existing `[ProducesResponseType]` attributes.

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Tests + coverage | `dotnet test --collect:"XPlat Code Coverage"` | 137 passed, 0 failed, 0 skipped; Services 100 % lines / 92.4 % branches |
| Build / format | `dotnet build --no-incremental`, `dotnet format --verify-no-changes` | 0 warnings, 0 errors; format clean |
| Swagger contents | `swagger.json` parsed per operation | 19 operations = the 18 ARCHITECTURE §6 endpoints + `GET /health`. 16 have the lock (all except register, login, health). Every operation lists request schema (where it has a body) and response codes/schemas (`200/201/204`, `400`, `401`, `404`, `423`, `429` as applicable) |
| Swagger UI | screenshot of `/swagger/index.html` | "Authorize" button; no lock on register/login; lock on category endpoints and the others |
| End-to-end pass | `curl` with the exact Swagger requests, in the task's order | register `201` → login `200` → category `201` → task `201` → 2 subtasks `201` → tick `200` (list `1/2`) → dashboard `200` (dueToday 1) → notifications `200` (1 DueSoon) → complete task `200` (dashboard completed 1) → delete category `204` (task `categoryId: null`) → delete task `204` (list `[]`) |
| Logs | new lines of `logs/todoapp-<date>.log` during the pass | 336 lines, 0 `ERR`/`WRN`/`FTL` |
