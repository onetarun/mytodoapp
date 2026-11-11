# Task 07 — SubTask (Checklist) API

**Depends on:** 06
**References:** FR-11 to FR-13, NFR-7, NFR-16 · ARCHITECTURE §6, §8
**Goal:** A user can add, edit, tick and delete checklist items inside their own task, and the task shows progress.

## Steps

### DTOs & validators
- [ ] `SubTaskDto` — `id`, `title`, `isCompleted`, `sortOrder`.
- [ ] `CreateSubTaskRequest` / `UpdateSubTaskRequest` — `title` (required, max 200).
- [ ] Reuse `SetCompletionRequest` from Task 06.

### Service (`ISubTaskService` / `SubTaskService`)
- [ ] Every method first loads the **parent task through the filtered `TodoTasks` set**. If it is not found (not the user's or deleted) → not-found. This enforces ownership (AD-5).
- [ ] `CreateAsync(taskId, request)` — `SortOrder = max + 1`.
- [ ] `UpdateAsync(taskId, subId, request)`.
- [ ] `SetCompletionAsync(taskId, subId, isCompleted)`.
- [ ] `DeleteAsync(taskId, subId)` — soft delete.
- [ ] Update `TaskService` so `TaskDetailDto.subTasks` (ordered by `SortOrder`) and the list's `subTaskTotal` / `subTaskCompleted` are filled (FR-13).

### Controller (`/api/v1/tasks/{taskId}/subtasks`)
- [ ] `POST /` → `201`.
- [ ] `PUT /{subId}` → `200` / `404`.
- [ ] `PATCH /{subId}/complete` → `200` / `404`.
- [ ] `DELETE /{subId}` → `204` / `404`.

### Tests
- [ ] Adding three subtasks gives sort orders 1, 2, 3.
- [ ] Ticking two of three gives progress `2/3` in the task list DTO.
- [ ] A deleted subtask is excluded from detail and from progress counts.
- [ ] User B cannot add, edit or delete subtasks on user A's task (not-found).
- [ ] A subtask id that belongs to a different task returns not-found.

## Definition of Done

- [ ] All four endpoints work in Swagger.
- [ ] `GET /tasks/{id}` shows subtasks; `GET /tasks` shows progress counts.
- [ ] All tests in this task pass.
