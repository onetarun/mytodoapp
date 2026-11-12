# Task 14 — Frontend: SubTasks / Checklist

**Depends on:** 13
**References:** FR-11 to FR-13 · US-4
**Goal:** On the task detail page, users manage a checklist and see progress; the list shows progress too.

## Steps

- [x] `SubTaskService`: `create(taskId)`, `update()`, `setCompletion()`, `delete()`.
- [x] Checklist section on `/tasks/:id`:
  - [x] Progress text and bar: "2 of 5 done" (FR-13).
  - [x] Each item: checkbox (done/undone), title, Edit (inline), Delete.
  - [x] "Add item" input at the bottom; Enter adds and keeps focus in the input for fast entry.
  - [x] Items shown in `sortOrder`.
  - [x] Delete a subtask without a dialog, but show an "Item deleted" toast. *(Lighter than task delete; change to a confirmation if the client prefers.)*
- [x] After any checklist change, the task list's "x/y" progress is up to date when the user goes back.

## Definition of Done

- [x] Add 3 items, tick 2 → detail shows "2 of 3 done" and the list shows "2/3". *(Browser, real API: 3 items added with Enter only, 2 ticked with Space → detail "2 of 3 done", bar 67%; Back to tasks → row "Checklist 2/3".)*
- [x] Edit and delete items work; deleted items don't count toward progress. *(Renamed "Book flights" → "Book return flights" (toast "Item updated."), Esc discards an edit; deleted the ticked "Book hotel" → toast "Item deleted", detail "1 of 2 done", list "1/2"; `sqlcmd`: the row has `IsDeleted = 1` (soft delete), order kept by `SortOrder` 1-2-3.)*
- [x] The checklist is fully usable with the keyboard. *(Whole flow done with Tab/Shift+Tab, Space, Enter and Esc only: focus stays in "Add item" after adding, returns to the item's Edit button after save/cancel, and moves to "Add item" after delete; empty Enter shows "Item text is required.")*
