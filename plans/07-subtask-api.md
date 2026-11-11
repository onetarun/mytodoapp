# Task 07 — SubTask (Checklist) API

**Depends on:** 06
**References:** FR-11 to FR-13, NFR-7, NFR-16 · ARCHITECTURE §6, §8
**Goal:** A user can add, edit, tick and delete checklist items inside their own task, and the task shows progress.

## Steps

### DTOs & validators
- [x] `SubTaskDto` — `id`, `title`, `isCompleted`, `sortOrder`.
- [x] `CreateSubTaskRequest` / `UpdateSubTaskRequest` — `title` (required, max 200).
- [x] Reuse `SetCompletionRequest` from Task 06.

### Service (`ISubTaskService` / `SubTaskService`)
- [x] Every method first loads the **parent task through the filtered `TodoTasks` set**. If it is not found (not the user's or deleted) → not-found. This enforces ownership (AD-5).
- [x] `CreateAsync(taskId, request)` — `SortOrder = max + 1`.
- [x] `UpdateAsync(taskId, subId, request)`.
- [x] `SetCompletionAsync(taskId, subId, isCompleted)`.
- [x] `DeleteAsync(taskId, subId)` — soft delete.
- [x] Update `TaskService` so `TaskDetailDto.subTasks` (ordered by `SortOrder`) and the list's `subTaskTotal` / `subTaskCompleted` are filled (FR-13).

### Controller (`/api/v1/tasks/{taskId}/subtasks`)
- [x] `POST /` → `201`.
- [x] `PUT /{subId}` → `200` / `404`.
- [x] `PATCH /{subId}/complete` → `200` / `404`.
- [x] `DELETE /{subId}` → `204` / `404`.

### Tests
- [x] Adding three subtasks gives sort orders 1, 2, 3.
- [x] Ticking two of three gives progress `2/3` in the task list DTO.
- [x] A deleted subtask is excluded from detail and from progress counts.
- [x] User B cannot add, edit or delete subtasks on user A's task (not-found).
- [x] A subtask id that belongs to a different task returns not-found.

## Definition of Done

- [x] All four endpoints work in Swagger.
- [x] `GET /tasks/{id}` shows subtasks; `GET /tasks` shows progress counts.
- [x] All tests in this task pass.

## Notes

- `SubTaskDto` was already added in Task 06 (`DTOs/Tasks`, same fields) as the element type of `TaskDetailDto.subTasks`. It is reused here, and only its doc comment changed.
- Ownership: every `SubTaskService` method first runs `AnyAsync` on the filtered `TodoTasks` set (current user, not deleted). The subtask is then looked up by **both** `Id` and `TodoTaskId`, so a subtask of another task gives not-found.
- `SortOrder` = max of the task's non-deleted subtasks + 1. The first subtask gets 1.
- Titles are trimmed. The create and update validators share `SubTaskTitleRuleExtensions` (required, max 200).
- `TaskService.GetByIdAsync` now projects the non-deleted subtasks ordered by `SortOrder`. The list's `subTaskTotal` / `subTaskCompleted` already counted non-deleted subtasks since Task 06.
- `POST` returns `Location: /api/v1/tasks/{taskId}/subtasks/{id}`.

### Verification (2026-11-11)

| Check | Command | Result |
|---|---|---|
| Build / format | `dotnet build --no-incremental`, `dotnet format --verify-no-changes` | 0 warnings, 0 errors; format clean |
| Tests | `dotnet test` | 124 passed (17 new), 0 failed, 0 skipped |
| Endpoints with JWT (real API + SQL Server) | `curl` against `http://localhost:5187` | no token `401`. `POST` → `201`, `Location: /api/v1/tasks/1004/subtasks/1`, sort orders 1, 2, 3. Blank title → `400`. `PUT` `200`. `PATCH complete` `200`. `DELETE` `204`, repeat `404` |
| Progress | `GET /tasks`, `GET /tasks/{id}` | 2 of 3 ticked → `subTaskTotal 3`, `subTaskCompleted 2`. After deleting a ticked one: `2` / `1`, and detail lists only `Book hostel`, `Pack` in sort order |
| Ownership / wrong task | user B; subtask id under another task id | all `404` |
| Soft delete in SQL | `sqlcmd` on `SubTasks` | deleted row kept with `IsDeleted = 1` |
| Swagger | `/swagger/v1/swagger.json` | three subtask routes listed. The UI click-through was not done (same calls verified with `curl`) |
