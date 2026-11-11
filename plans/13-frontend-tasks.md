# Task 13 — Frontend: Tasks

**Depends on:** 12
**References:** FR-6 to FR-10a, FR-21, NFR-21, NFR-24 · US-2, US-3, US-6
**Goal:** The main task screen: quick add, list with status filter, edit, complete and delete, with overdue tasks clearly marked.

## Steps

### Service
- [x] `TaskService`: `getAll(status)`, `getById(id)`, `create()`, `update()`, `setCompletion()`, `delete()`.

### Task list page (`/tasks`)
- [x] **Quick add** at the top: title input + Enter creates a task with defaults (≤ 2 clicks, NFR-21). An "Add details" toggle reveals description, due date/time, priority and category fields.
- [x] **Status filter**: All / Pending / Completed (segmented buttons or tabs), default All (FR-10a).
  - [x] The selected filter **stays applied** after add, edit, complete and delete.
  - [x] Keep it in the URL query (`?status=pending`) so refresh keeps it.
- [x] **Task row** shows (FR-10): checkbox, title, priority badge (High/Medium/Low with text, not color only), category name or "Uncategorized", due date in local time, checklist progress "2/5" if any.
- [x] **Checkbox** toggles complete/incomplete immediately (FR-9); completed tasks show a strikethrough.
- [x] **Overdue**: red highlight **plus** a warning icon and "Overdue" text (FR-21, NFR-24).
- [x] Row actions: Edit, Delete. Clicking the title opens `/tasks/:id`.
- [x] **Delete** asks for confirmation first (FR-8).
- [x] Empty states per filter ("No pending tasks 🎉", "No tasks yet — add your first one").
- [x] Loading indicator while fetching.

### Task form (create/edit — dialog or `/tasks/:id`)
- [x] Fields: Title (required, max 200), Description (max 2000), Due date + time (`datetime-local`, converted to UTC ISO before sending), Priority (default Medium), Category (dropdown from `CategoryService`, includes "Uncategorized").
- [x] Validation messages match the API rules; show server errors.

### Task detail page (`/tasks/:id`)
- [x] Shows all fields and an Edit button; the subtask area is a placeholder for Task 14.
- [x] `404` from the API → "Task not found" with a link back to the list.

## Definition of Done

- [x] Typing a title and pressing Enter creates a task.
- [x] Filter All / Pending / Completed shows the right tasks and survives completing a task and a page refresh.
- [x] A past-due pending task shows red + icon + "Overdue"; completing it removes the overdue marker.
- [x] Edit and delete (with confirmation) work.
- [x] Due dates display in local time and are saved correctly (check UTC in SSMS).

## Notes

- `TaskService` lives in `features/tasks/task.service.ts`. Limits, priorities and status values are in `task-rules.ts`, together with `parseStatusFilter`, which turns any unknown `?status=` into `all`.
- **`TaskFormComponent`** is one reactive form used both for quick add (title only, Enter creates) and for "Add details" / the edit dialog (all fields). The parent passes a `save` function; the form shows field errors from the server and emits `saved`.
  - Category options come from the cached `CategoryService.categories` (Task 12).
  - `null` = "Uncategorized", shown through the select placeholder.
- **Dates:** `shared/dates/date-time.ts` turns `datetime-local` (browser local time) into UTC ISO, and back. The list and detail pages show dates with `DatePipe`, which uses local time.
- **Status filter:** a Material button-toggle group bound to `?status=` (`all` removes the param). The list reloads through an `effect` on the query input, so the filter stays applied after add, edit, complete, delete and refresh.
- **Overdue (FR-21, NFR-24):** red left border and background, plus a warning icon and the text "Overdue". Priority is a pill with text (`PriorityBadgeComponent`, also used on the detail page); screen readers hear "High priority".
- **Delete:** reuses the confirm dialog from Task 12. **Edit:** opens `TaskEditDialogComponent` from the list row and from the detail page, and focus returns to the trigger afterwards.
- **Detail page:** `/tasks/:id` shows every field. The checklist area shows `x/y done` until Task 14. A `404` or a non-numeric id shows "Task not found" with a link back to the list.
- **Bundle budget (client decision 2026-11-11):** the initial warning budget was raised from 500 kB to 650 kB. The initial bundle is now 502.66 kB raw / ~125 kB gzip; the Angular core chunk shared by the lazy pages counts as initial. Logged in the GUARDRAILS change log.
  - The shell's tooltip on the disabled bell was removed. A disabled button cannot show a tooltip, and removing it saved ~18 kB.

### Verification (2026-11-11, `ng serve` + API `https` profile, browser pane, keyboard input; browser time zone UTC+5:30)

| Check | Result |
|---|---|
| Lint / Prettier / build | all pass; no build warnings with the approved 650 kB budget (initial 502.66 kB) |
| Quick add: type "Buy groceries" + Enter | task appears without a reload; toast "Task added."; field cleared and focused |
| Blank title | "Title is required.", focus stays on the title |
| Overdue task (due 2 h ago, seeded through the API) | red row, ⚠ icon and "Overdue" text, "High" badge, "Uncategorized", local due time |
| Filter Pending → Space on the overdue task's checkbox | URL `?status=pending`; task leaves the pending list; toast "Task completed." |
| Refresh `?status=pending` | Pending still selected, same list |
| Completed filter | task shown ticked with line-through and **no** overdue marker |
| Add details: due 15-12-2026 09:30, High, Work | API `dueDate: 2026-12-15T04:00:00Z`, `categoryName: Work`; list shows "Dec 15, 2026, 9:30:00 AM" |
| Edit dialog | fields prefilled (due `2026-12-15T09:30`); title changed + Enter → saved, toast, focus back on the Edit button. SQL after the edit: `DueDate 2026-12-15 04:00:00`, Priority 2 (High), CategoryId set |
| Delete | dialog "Delete 'Buy groceries'? …"; Esc keeps the task; Delete removes it; toast "Task deleted." |
| Checklist progress | "Plan trip" row shows `1/2`; detail page "1/2 done" |
| Detail page | Status, Priority, Category, Due, Description, Created, Last updated, Edit button |
| `/tasks/999999` and `/tasks/abc` | "Task not found" with a link to `/tasks` |
| Empty states (new user) | All: "No tasks yet — add your first one"; Pending: "No pending tasks 🎉"; Completed: "No completed tasks yet"; `?status=bogus` → All |
| Console | no app errors (`TypeError` / `ERROR` / `NG0` filter: none) |
