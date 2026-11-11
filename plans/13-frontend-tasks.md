# Task 13 — Frontend: Tasks

**Depends on:** 12
**References:** FR-6 to FR-10a, FR-21, NFR-21, NFR-24 · US-2, US-3, US-6
**Goal:** The main task screen: quick add, list with status filter, edit, complete and delete, with overdue tasks clearly marked.

## Steps

### Service
- [ ] `TaskService`: `getAll(status)`, `getById(id)`, `create()`, `update()`, `setCompletion()`, `delete()`.

### Task list page (`/tasks`)
- [ ] **Quick add** at the top: title input + Enter creates a task with defaults (≤ 2 clicks, NFR-21). An "Add details" toggle reveals description, due date/time, priority and category fields.
- [ ] **Status filter**: All / Pending / Completed (segmented buttons or tabs), default All (FR-10a).
  - [ ] The selected filter **stays applied** after add, edit, complete and delete.
  - [ ] Keep it in the URL query (`?status=pending`) so refresh keeps it.
- [ ] **Task row** shows (FR-10): checkbox, title, priority badge (High/Medium/Low with text, not color only), category name or "Uncategorized", due date in local time, checklist progress "2/5" if any.
- [ ] **Checkbox** toggles complete/incomplete immediately (FR-9); completed tasks show a strikethrough.
- [ ] **Overdue**: red highlight **plus** a warning icon and "Overdue" text (FR-21, NFR-24).
- [ ] Row actions: Edit, Delete. Clicking the title opens `/tasks/:id`.
- [ ] **Delete** asks for confirmation first (FR-8).
- [ ] Empty states per filter ("No pending tasks 🎉", "No tasks yet — add your first one").
- [ ] Loading indicator while fetching.

### Task form (create/edit — dialog or `/tasks/:id`)
- [ ] Fields: Title (required, max 200), Description (max 2000), Due date + time (`datetime-local`, converted to UTC ISO before sending), Priority (default Medium), Category (dropdown from `CategoryService`, includes "Uncategorized").
- [ ] Validation messages match the API rules; show server errors.

### Task detail page (`/tasks/:id`)
- [ ] Shows all fields and an Edit button; the subtask area is a placeholder for Task 14.
- [ ] `404` from the API → "Task not found" with a link back to the list.

## Definition of Done

- [ ] Typing a title and pressing Enter creates a task.
- [ ] Filter All / Pending / Completed shows the right tasks and survives completing a task and a page refresh.
- [ ] A past-due pending task shows red + icon + "Overdue"; completing it removes the overdue marker.
- [ ] Edit and delete (with confirmation) work.
- [ ] Due dates display in local time and are saved correctly (check UTC in SSMS).
