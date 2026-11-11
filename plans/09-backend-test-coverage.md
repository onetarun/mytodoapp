# Task 09 — Backend Test Coverage Gate

**Depends on:** 08
**References:** NFR-26, NFR-27 · ARCHITECTURE §11
**Goal:** Confirm the backend is complete and stable before frontend work starts: ≥ 60% coverage on services and every endpoint documented in Swagger.

## Steps

- [ ] Run `dotnet test --collect:"XPlat Code Coverage"`.
- [ ] Generate a readable report (e.g. `reportgenerator`) and check coverage of the `Services/` namespace.
- [ ] If coverage is below **60%**, add tests for the uncovered branches (validation failures, not-found paths, edge cases).
- [ ] Review Swagger: every endpoint from ARCHITECTURE §6 is listed with request/response schemas and the Bearer lock icon (except register/login).
- [ ] Add XML comments or `[ProducesResponseType]` attributes so status codes show in Swagger.
- [ ] Do one manual end-to-end pass in Swagger: register → login → create category → create task → add subtasks → complete → dashboard → notifications → delete category → delete task.
- [ ] Check `logs/` has no unexpected errors from the manual pass.
- [ ] Commit the backend as a stable baseline.

## Definition of Done

- [ ] All backend tests pass.
- [ ] Service-layer coverage ≥ 60% (record the number below).
- [ ] All 18 endpoints are documented in Swagger.
- [ ] Manual Swagger pass completed with no errors.

## Notes

- Coverage achieved: `____ %`
