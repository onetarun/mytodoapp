# Task 06 — Task API

**Depends on:** 05
**References:** FR-6 to FR-10a, FR-21, NFR-1, NFR-7, NFR-16 · ARCHITECTURE §6, §8
**Goal:** A logged-in user can create, view, edit, complete/uncomplete, filter by status and soft delete their own tasks.

## Steps

### DTOs & validators
- [x] `TaskListItemDto` — `id`, `title`, `priority`, `categoryId`, `categoryName`, `dueDate`, `isCompleted`, `isOverdue`, `subTaskTotal`, `subTaskCompleted`.
- [x] `TaskDetailDto` — list fields + `description`, `completedAt`, `createdAt`, `updatedAt`, `subTasks[]` (empty until Task 07).
- [x] `CreateTaskRequest` / `UpdateTaskRequest` — `title` (required, max 200), `description` (max 2000), `dueDate?`, `priority` (default Medium), `categoryId?`.
- [x] Validator: if `categoryId` is given, it must belong to the current user (otherwise `400`).
- [x] `SetCompletionRequest` — `isCompleted` (bool).
- [x] `TaskStatusFilter` enum — `All`, `Pending`, `Completed`.

### Service (`ITaskService` / `TaskService`)
- [x] `GetAllAsync(status)` — filter by status (FR-10a); order: pending first, then by `DueDate` (nulls last), then `CreatedAt` desc.
- [x] Compute **`isOverdue`** = `!IsCompleted && DueDate < UtcNow` (FR-21) in one shared helper (reused by Task 08).
- [x] Project to DTOs in the query (`Select`), using `AsNoTracking()` for reads (NFR-1).
- [x] `GetByIdAsync(id)` → detail or not-found.
- [x] `CreateAsync(request)` — `UserId` from the token.
- [x] `UpdateAsync(id, request)` — updates all editable fields; `UpdatedAt` set automatically.
- [x] `SetCompletionAsync(id, isCompleted)` — sets `CompletedAt = UtcNow` when completed, `null` when reopened.
- [x] `DeleteAsync(id)` — soft delete (NFR-7).

### Controller (`/api/v1/tasks`)
- [x] `GET /?status=all|pending|completed` → `200` (invalid status → `400`).
- [x] `GET /{id}` → `200` / `404`.
- [x] `POST /` → `201` + `Location`.
- [x] `PUT /{id}` → `200` / `404`.
- [x] `PATCH /{id}/complete` → `200` / `404`.
- [x] `DELETE /{id}` → `204` / `404`.

### Tests
- [x] Create with only a title → priority defaults to Medium, not completed.
- [x] Title empty or > 200 chars is rejected.
- [x] Using another user's `categoryId` is rejected.
- [x] Status filter returns the right sets for All / Pending / Completed.
- [x] Complete sets `CompletedAt`; reopen clears it.
- [x] `isOverdue` is true only for past-due, not-completed tasks.
- [x] Soft-deleted tasks don't appear in the list or detail.
- [x] User B gets not-found for user A's task on get, update, complete and delete.
- [x] After deleting a category (Task 05), its tasks show `categoryId = null`.

## Definition of Done

- [x] All six endpoints work in Swagger.
- [x] `GET /tasks` with 1,000 seeded tasks responds in < 500 ms locally (NFR-1).
- [x] All tests in this task pass.

## Notes

- **Overdue rule:** `Services/TaskRules` has `IsOverdue(isCompleted, dueDate, utcNow)` for DTO mapping and `IsOverdueExpression(utcNow)` for SQL filters (Task 08). A test checks that both give the same result.
- **Reads:** list and detail are projected to DTOs in the query with `AsNoTracking()`. `isOverdue` is set after the query from one `UtcNow` value.
- **List order:** pending first, then `DueDate` ascending with no due date last, then `CreatedAt` descending.
- **Validation:** create and update share `TaskWriteRequestValidator<T>` through `ITaskWriteRequest`. The rules are: title required (not blank) and max 200, description max 2000, defined priority, and the category must exist for the current user. The category check runs `AnyAsync` through the ownership/soft-delete filter, so another user's category or a deleted one gives `400 errors.categoryId`.
- **Status filter:** `?status=` binds case-insensitively. An unknown name gives `400` from model binding. A numeric value outside the enum (e.g. `7`) gives `400` from the controller.
- **Completion:** a repeated "complete" keeps the original `CompletedAt`. "Reopen" clears it.
- **Subtasks:** `subTaskTotal` / `subTaskCompleted` already count non-deleted subtasks. `subTasks[]` is always empty, and Task 07 fills it. `SubTaskDto` (fields from Task 07) is defined now because `TaskDetailDto` needs the type.
- **Due dates:** a due date with an offset (e.g. `+05:30`) is stored and returned in UTC.

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Build / format | `dotnet build --no-incremental`, `dotnet format --verify-no-changes` | 0 warnings, 0 errors; format clean |
| Tests | `dotnet test` | 107 passed (31 new), 0 failed, 0 skipped |
| Six endpoints with JWT (real API + SQL Server) | `curl` against `http://localhost:5187` | `POST` title only → `201`, `Location: /api/v1/tasks/2`, Medium, not completed. Full create → `201`, `categoryName` Work, `dueDate` `+05:30` stored as `04:30Z`, `isOverdue` true. `GET` list/detail `200`. `PUT` `200` (`updatedAt` changed). `PATCH complete` `200` with `completedAt`; reopen → `completedAt: null`. `DELETE` `204`, then `GET` `404` |
| Validation | same | other user's category → `400 errors.categoryId`; blank title → `400`; priority `Urgent` → `400`; `?status=bogus` / `?status=7` → `400`; `pending` / `Completed` → `200` |
| Ownership | user B on user A's task | `GET`/`PUT`/`PATCH`/`DELETE` → `404` |
| Soft delete in SQL | `sqlcmd` | deleted task row kept, `IsDeleted = 1`, `DeletedAt` set |
| NFR-1 | 1,000 tasks seeded with `sqlcmd` for one user, `curl -w %{time_total}` on `GET /tasks` | 1,000 items returned; 6 runs 0.011–0.025 s; `?status=pending` 0.047 s (all < 0.5 s) |
| Swagger | `/swagger/v1/swagger.json` | `/api/v1/tasks`, `/api/v1/tasks/{id}`, `/api/v1/tasks/{id}/complete` listed. The UI click-through was not done (same calls verified with `curl`) |
