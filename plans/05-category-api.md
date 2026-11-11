# Task 05 — Category API

**Depends on:** 04
**References:** FR-14 to FR-16, NFR-7, NFR-16 · ARCHITECTURE §6, §8
**Goal:** A logged-in user can list, create, rename and delete (soft) their own categories.

## Steps

### DTOs & validators
- [x] `CategoryDto` (`id`, `name`, `taskCount`).
- [x] `CreateCategoryRequest` / `UpdateCategoryRequest` (`name`) — required, max 50, trimmed.
- [x] Rule: category name must be **unique per user** (case-insensitive) among non-deleted categories.

### Service (`ICategoryService` / `CategoryService`)
- [x] `GetAllAsync()` — current user's categories, ordered by name, with the count of non-deleted tasks.
- [x] `CreateAsync(request)` — `UserId` from `ICurrentUserService`, never from the request.
- [x] `UpdateAsync(id, request)` — returns not-found if the category is not the user's.
- [x] `DeleteAsync(id)`:
  - set `CategoryId = null` on all the user's tasks in this category (they become **Uncategorized**, FR-16);
  - soft delete the category;
  - do both in **one `SaveChangesAsync`** (single transaction).

### Controller (`/api/v1/categories`)
- [x] `GET /` → `200`.
- [x] `POST /` → `201` + `Location`.
- [x] `PUT /{id}` → `200` / `404`.
- [x] `DELETE /{id}` → `204` / `404`.

### Tests
- [x] Create, rename and list work for the owner.
- [x] Duplicate name for the same user is rejected; the same name for a different user is allowed.
- [x] Deleting a category soft-deletes it and sets its tasks' `CategoryId` to null (tasks are **not** deleted).
- [x] User B gets not-found for user A's category id on update and delete.

## Definition of Done

- [x] All four endpoints work in Swagger with a JWT.
- [x] A new user sees **Work** and **Personal** from `GET /categories`.
- [x] After delete, the row still exists in SQL with `IsDeleted = 1`.
- [x] All tests in this task pass.

## Notes

- `ServiceResult<T>` (`Success` / `NotFound` / `ValidationFailed`) is the shared result type for feature services; the controller maps it to `200`/`201`, `404` and `400`.
- Name rules: `CategoryNameRuleExtensions.ValidCategoryName` (required, max 50 after trimming) is used by both request validators. The service trims the name before saving.
- Uniqueness is checked in `CategoryService` with `Name.ToLower()` on both sides, so it is case-insensitive on SQL Server and on the InMemory test provider. The query filter limits the check to the user's non-deleted categories, so a deleted name can be reused and another user's names do not count.
- `taskCount` counts the user's non-deleted tasks (the `TodoTask` query filter plus an explicit `!IsDeleted`).
- Delete loads the user's tasks in the category, sets `CategoryId = null`, and removes the category. `AppDbContext` turns that into a soft delete, and everything is written by one `SaveChangesAsync`.
- `POST` returns `Location: /api/v1/categories/{id}`. There is no `GET /categories/{id}` in ARCHITECTURE §6, so the location identifies the new resource for `PUT`/`DELETE`.

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Build / format | `dotnet build --no-incremental`, `dotnet format --verify-no-changes` | 0 warnings, 0 errors; format clean |
| Tests | `dotnet test` | 76 passed (27 new), 0 failed, 0 skipped |
| Endpoints with JWT (real API + SQL Server) | `curl` against `http://localhost:5187` | no token `401`; new user `GET` → `Personal`, `Work`; `POST "  Home  "` → `201`, `Location: /api/v1/categories/7`, name trimmed; duplicate `work` → `400 errors.name`; blank name → `400`; `PUT` → `200`; repeated `DELETE` → `404` |
| Ownership | user B on user A's category id | `PUT` → `404`, `DELETE` → `404`; B's `GET` shows only B's categories |
| FR-16 on SQL Server | task inserted with `sqlcmd` into a user's `Work`, then `DELETE /categories/{id}` | `204`; `Categories` row kept with `IsDeleted = 1` and `DeletedAt` set; task row kept with `CategoryId = NULL`, `IsDeleted = 0` |
| Swagger | `/swagger/v1/swagger.json` | `/api/v1/categories` and `/api/v1/categories/{id}` listed. The UI click-through was not done (same calls verified with `curl`) |
