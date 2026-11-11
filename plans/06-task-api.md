# Task 06 — Task API

**Depends on:** 05
**References:** FR-6 to FR-10a, FR-21, NFR-1, NFR-7, NFR-16 · ARCHITECTURE §6, §8
**Goal:** A logged-in user can create, view, edit, complete/uncomplete, filter by status and soft delete their own tasks.

## Steps

### DTOs & validators
- [ ] `TaskListItemDto` — `id`, `title`, `priority`, `categoryId`, `categoryName`, `dueDate`, `isCompleted`, `isOverdue`, `subTaskTotal`, `subTaskCompleted`.
- [ ] `TaskDetailDto` — list fields + `description`, `completedAt`, `createdAt`, `updatedAt`, `subTasks[]` (empty until Task 07).
- [ ] `CreateTaskRequest` / `UpdateTaskRequest` — `title` (required, max 200), `description` (max 2000), `dueDate?`, `priority` (default Medium), `categoryId?`.
- [ ] Validator: if `categoryId` is given, it must belong to the current user (otherwise `400`).
- [ ] `SetCompletionRequest` — `isCompleted` (bool).
- [ ] `TaskStatusFilter` enum — `All`, `Pending`, `Completed`.

### Service (`ITaskService` / `TaskService`)
- [ ] `GetAllAsync(status)` — filter by status (FR-10a); order: pending first, then by `DueDate` (nulls last), then `CreatedAt` desc.
- [ ] Compute **`isOverdue`** = `!IsCompleted && DueDate < UtcNow` (FR-21) in one shared helper (reused by Task 08).
- [ ] Project to DTOs in the query (`Select`), using `AsNoTracking()` for reads (NFR-1).
- [ ] `GetByIdAsync(id)` → detail or not-found.
- [ ] `CreateAsync(request)` — `UserId` from the token.
- [ ] `UpdateAsync(id, request)` — updates all editable fields; `UpdatedAt` set automatically.
- [ ] `SetCompletionAsync(id, isCompleted)` — sets `CompletedAt = UtcNow` when completed, `null` when reopened.
- [ ] `DeleteAsync(id)` — soft delete (NFR-7).

### Controller (`/api/v1/tasks`)
- [ ] `GET /?status=all|pending|completed` → `200` (invalid status → `400`).
- [ ] `GET /{id}` → `200` / `404`.
- [ ] `POST /` → `201` + `Location`.
- [ ] `PUT /{id}` → `200` / `404`.
- [ ] `PATCH /{id}/complete` → `200` / `404`.
- [ ] `DELETE /{id}` → `204` / `404`.

### Tests
- [ ] Create with only a title → priority defaults to Medium, not completed.
- [ ] Title empty or > 200 chars is rejected.
- [ ] Using another user's `categoryId` is rejected.
- [ ] Status filter returns the right sets for All / Pending / Completed.
- [ ] Complete sets `CompletedAt`; reopen clears it.
- [ ] `isOverdue` is true only for past-due, not-completed tasks.
- [ ] Soft-deleted tasks don't appear in the list or detail.
- [ ] User B gets not-found for user A's task on get, update, complete and delete.
- [ ] After deleting a category (Task 05), its tasks show `categoryId = null`.

## Definition of Done

- [ ] All six endpoints work in Swagger.
- [ ] `GET /tasks` with 1,000 seeded tasks responds in < 500 ms locally (NFR-1).
- [ ] All tests in this task pass.
