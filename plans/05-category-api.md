# Task 05 — Category API

**Depends on:** 04
**References:** FR-14 to FR-16, NFR-7, NFR-16 · ARCHITECTURE §6, §8
**Goal:** A logged-in user can list, create, rename and delete (soft) their own categories.

## Steps

### DTOs & validators
- [ ] `CategoryDto` (`id`, `name`, `taskCount`).
- [ ] `CreateCategoryRequest` / `UpdateCategoryRequest` (`name`) — required, max 50, trimmed.
- [ ] Rule: category name must be **unique per user** (case-insensitive) among non-deleted categories.

### Service (`ICategoryService` / `CategoryService`)
- [ ] `GetAllAsync()` — current user's categories, ordered by name, with the count of non-deleted tasks.
- [ ] `CreateAsync(request)` — `UserId` from `ICurrentUserService`, never from the request.
- [ ] `UpdateAsync(id, request)` — returns not-found if the category is not the user's.
- [ ] `DeleteAsync(id)`:
  - set `CategoryId = null` on all the user's tasks in this category (they become **Uncategorized**, FR-16);
  - soft delete the category;
  - do both in **one `SaveChangesAsync`** (single transaction).

### Controller (`/api/v1/categories`)
- [ ] `GET /` → `200`.
- [ ] `POST /` → `201` + `Location`.
- [ ] `PUT /{id}` → `200` / `404`.
- [ ] `DELETE /{id}` → `204` / `404`.

### Tests
- [ ] Create, rename and list work for the owner.
- [ ] Duplicate name for the same user is rejected; the same name for a different user is allowed.
- [ ] Deleting a category soft-deletes it and sets its tasks' `CategoryId` to null (tasks are **not** deleted).
- [ ] User B gets not-found for user A's category id on update and delete.

## Definition of Done

- [ ] All four endpoints work in Swagger with a JWT.
- [ ] A new user sees **Work** and **Personal** from `GET /categories`.
- [ ] After delete, the row still exists in SQL with `IsDeleted = 1`.
- [ ] All tests in this task pass.
