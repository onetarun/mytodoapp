# Task 12 — Frontend: Categories

**Depends on:** 11
**References:** FR-14 to FR-16 · US-5
**Goal:** Users can view, add, rename and delete their categories; the category list is available for the task form in Task 13.

## Steps

- [x] `CategoryService` (feature service): `getAll()`, `create()`, `update()`, `delete()`, plus a cached `categories` signal that other features (task form, task list) can read and that refreshes after any change.
- [x] `/categories` page:
  - [x] List of categories with name and task count.
  - [x] Inline "Add category" input + button (Enter to submit).
  - [x] Rename: inline edit with Save / Cancel (Esc cancels).
  - [x] Delete: confirmation dialog — "Delete '<name>'? Its N tasks will become Uncategorized." (FR-16)
  - [x] Validation: required, max 50; show the server's duplicate-name error.
  - [x] Empty state: "No categories yet".
- [x] Success and error toasts for every action.

## Definition of Done

- [x] A new user sees Work and Personal.
- [x] Add, rename and delete work and the list updates without a page reload.
- [x] After deleting a category, its tasks still exist (check via API) with no category.
- [x] Duplicate names show a clear error.

## Notes

- **`CategoryService`** (`features/categories/category.service.ts`, `providedIn: 'root'`):
  - Exposes the read-only `categories` and `isLoaded` signals, plus `getAll()`, `ensureLoaded()` (for the task form and list in Task 13), `create()`, `update()` and `delete()`.
  - Each change re-reads the list, so task counts stay correct.
  - The cache is cleared when the logged-in user changes, so one user never sees another user's categories.
- **Shared confirm dialog** (`shared/confirm-dialog/`): `ConfirmService.confirm({ title, message, confirmLabel })` opens a Material `alertdialog`.
  - Focus starts on Cancel, Esc cancels, and focus returns to the trigger.
  - It is reused for task delete (FR-8) in Task 13.
- **Validation:** `shared/validators/not-blank.validator.ts` makes a name of only spaces count as empty. Max length is `CATEGORY_NAME_MAX_LENGTH = 50`.
- **Server errors:**
  - A duplicate name shows under the field and as an error toast.
  - A `404` (category deleted elsewhere) gives a toast and reloads the list.
  - Network, 5xx and 429 errors are toasted by the interceptor.
- **Keyboard flow:**
  - Add: Enter submits.
  - Rename: Enter on ✎ opens the inline edit with the text selected; Enter saves, Esc cancels. Focus then returns to that row's ✎ button.
  - Delete: Enter on 🗑 opens the dialog. After a delete, focus goes to the "New category" field.
- **Bug found while testing:** the add/rename `<form>`s had no `[formGroup]`, so Enter caused a native page reload. Both forms are now bound to a `FormGroup` (commit `6fca277`).

### Verification (2026-11-11, `ng serve` + API `https` profile, browser pane, keyboard input)

| Check | Result |
|---|---|
| Lint / Prettier / build | all pass; build has no warnings, initial 481 kB; the categories page is a lazy chunk |
| New user | `/categories` lists Personal (0 tasks) and Work (1 task, seeded through the API). The list call carries the Bearer token (`authInterceptor` works) |
| Add "Home" with Enter | appears in the list without a reload; field cleared and focused; toast `Category "Home" added.` |
| Duplicate "work" | under the field and as a toast: "You already have a category with this name."; list unchanged |
| Blank name (spaces) | "Name is required." |
| Rename → Esc | edit closes, name unchanged, focus back on "Rename Home" |
| Rename → "Family" + Enter | list shows Family; toast `Category renamed to "Family".`; focus on "Rename Family" |
| Delete Work → dialog | `alertdialog` "Delete category" / "Delete 'Work'? Its 1 task will become Uncategorized."; focus on Cancel; Esc → nothing deleted, focus back on "Delete Work" |
| Delete Work → confirm | removed from the list; toast `Category "Work" deleted.`; API: categories Family, Personal; task "Quarterly report" still exists with `categoryId: null` |
| Delete the rest | empty state "No categories yet" |
