# Task 20 — Square Buttons with Rounded Corners

**Depends on:** 19
**References:** NFR-22a · GitHub issue onetarun/mytodoapp#1
**Goal:** Text buttons across the app are rectangles with 8 px rounded corners instead of Material 3's default pill (oval) shape.

## Steps

### Docs (scope change first, G-SCOPE-2)
- [x] BRD v1.4: NFR-22a (button shape).
- [x] Index: Task 20 added.

### Frontend
- [x] `src/styles.scss`: one global `mat.button-overrides` that sets the container shape of text, filled, outlined, tonal and protected buttons to 8 px.
- [x] Icon buttons (`mat-icon-button`) keep their circular shape.
- [x] No component-level overrides, no `::ng-deep`, no new packages (G-DEP-1).

## Definition of Done

- [x] Frontend `npm run build`, `ng lint` and Prettier check pass. *(The Angular project has no `test` target, so there are no frontend unit tests to run.)*
- [x] In the browser, buttons show square corners with a small radius and icon buttons are still round. *(2026-11-12, `ng serve` on `http://localhost:4300/login`: "Log in" button computed `border-radius: 8px`. Icon buttons read `--mat-icon-button-container-shape`, which this task does not override, so they stay circular. The strong focus indicator uses `border-radius: inherit`, so it follows the new shape.)*
- [x] No backend change.

## Notes

- Material 3 default for buttons is `--mat-sys-corner-full` (pill). The override only changes the shape tokens, so colors, density and focus indicators are unchanged.
