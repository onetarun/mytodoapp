# Task 14 — Frontend: SubTasks / Checklist

**Depends on:** 13
**References:** FR-11 to FR-13 · US-4
**Goal:** On the task detail page, users manage a checklist and see progress; the list shows progress too.

## Steps

- [ ] `SubTaskService`: `create(taskId)`, `update()`, `setCompletion()`, `delete()`.
- [ ] Checklist section on `/tasks/:id`:
  - [ ] Progress text and bar: "2 of 5 done" (FR-13).
  - [ ] Each item: checkbox (done/undone), title, Edit (inline), Delete.
  - [ ] "Add item" input at the bottom; Enter adds and keeps focus in the input for fast entry.
  - [ ] Items shown in `sortOrder`.
  - [ ] Delete a subtask without a dialog, but show an "Item deleted" toast. *(Lighter than task delete; change to a confirmation if the client prefers.)*
- [ ] After any checklist change, the task list's "x/y" progress is up to date when the user goes back.

## Definition of Done

- [ ] Add 3 items, tick 2 → detail shows "2 of 3 done" and the list shows "2/3".
- [ ] Edit and delete items work; deleted items don't count toward progress.
- [ ] The checklist is fully usable with the keyboard.
