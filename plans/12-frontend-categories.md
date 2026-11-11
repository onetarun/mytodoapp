# Task 12 — Frontend: Categories

**Depends on:** 11
**References:** FR-14 to FR-16 · US-5
**Goal:** Users can view, add, rename and delete their categories; the category list is available for the task form in Task 13.

## Steps

- [ ] `CategoryService` (feature service): `getAll()`, `create()`, `update()`, `delete()`, plus a cached `categories` signal that other features (task form, task list) can read and that refreshes after any change.
- [ ] `/categories` page:
  - [ ] List of categories with name and task count.
  - [ ] Inline "Add category" input + button (Enter to submit).
  - [ ] Rename: inline edit with Save / Cancel (Esc cancels).
  - [ ] Delete: confirmation dialog — "Delete '<name>'? Its N tasks will become Uncategorized." (FR-16)
  - [ ] Validation: required, max 50; show the server's duplicate-name error.
  - [ ] Empty state: "No categories yet".
- [ ] Success and error toasts for every action.

## Definition of Done

- [ ] A new user sees Work and Personal.
- [ ] Add, rename and delete work and the list updates without a page reload.
- [ ] After deleting a category, its tasks still exist (check via API) with no category.
- [ ] Duplicate names show a clear error.
