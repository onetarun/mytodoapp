# Task 16 — Responsive & Accessibility Polish

**Depends on:** 15
**References:** NFR-2, NFR-19 to NFR-25
**Goal:** The whole app looks right from 360 px to 1920 px, works with the keyboard, and meets the basic accessibility rules.

## Steps

### Responsive (NFR-20)
- [x] Check every page at **360, 768, 1280 and 1920 px** (browser dev tools). *(Headless Chrome 157 script: Login, Register, Dashboard, Tasks, Task detail, Categories, edit dialog and bell menu at all four widths; screenshots reviewed.)*
- [x] Header collapses into a menu on small screens; the bell stays visible. *(Below 768 px: app name, bell and a "Main menu" button with Dashboard / Tasks / Categories / Logout and the user name.)*
- [x] Task rows wrap cleanly; actions stay reachable (≥ 44 px touch targets). *(Icon buttons and checkboxes have 48 px targets; task and upcoming title links now 44 px high.)*
- [x] Dashboard cards: 1 column on mobile, 2–3 on tablet, 5 on desktop. *(< 600 px: 1; 600–1023 px: 3; ≥ 1024 px: 5.)*
- [x] Dialogs fit the screen on mobile; no horizontal scrolling anywhere. *(`scrollWidth − clientWidth = 0` on every page and width, including the open dialog and menus; it was 244 px at 360 px before the header fix.)*

### Accessibility (NFR-23, NFR-24)
- [x] Tab through every page: logical order, **visible focus ring**, no keyboard traps; dialogs trap focus and return it on close. *(Fixed: Material strong focus indicators, skip link focusing `<main>`, `disabledInteractive` on busy row actions so the edit dialog returns focus to Edit.)*
- [x] All inputs have labels; icon-only buttons have `aria-label`. *(Lighthouse `label`, `button-name`, `link-name` pass.)*
- [x] Color contrast ≥ 4.5:1 for text (check with Lighthouse or an axe browser extension). *(Lighthouse `color-contrast` passes on all three pages, desktop and mobile.)*
- [x] Overdue, priority and completed states are shown with text/icons, not color alone. *(Overdue: red + warning icon + "Overdue"; priority: text badge; completed: ticked checkbox + strike-through + "Mark as pending" label; bell overdue items now also have a red icon.)*
- [x] Run **Lighthouse Accessibility** on Login, Dashboard and Tasks; fix issues until the score is ≥ 90. *(Lighthouse 12.8.2: 100 / 100 / 100 on desktop and mobile.)*

### Performance (NFR-2)
- [x] `ng build` (production) — check bundle sizes; confirm feature routes are lazy-loaded. *(Initial 559.27 kB raw / 132.92 kB transfer, under the 650 kB budget; auth, dashboard, tasks and categories are lazy chunks.)*
- [x] With 1,000 tasks, the task list and dashboard load in < 2 s (Lighthouse or network tab). *(Production build served with gzip: task list 928–971 ms until all 1,000 rows show, dashboard 308–362 ms, cold with cache off; Lighthouse performance 93 / 100.)*

### Browsers (NFR-25)
- [x] Smoke test the full flow in the latest **Chrome** and **Edge**. *(Chrome 157: full keyboard-only flow passes at 1280 and 360 px. Edge 157: done by the client (decision 2026-11-12).)*

### UI consistency (NFR-19)
- [x] Consistent spacing, fonts and buttons; minimal and clean look; English-only text (NFR-22). *(Shared spacing tokens, Roboto, Material buttons; screenshots reviewed.)*

## Definition of Done

- [x] No layout breaks at 360 / 768 / 1280 / 1920 px. *(0 px overflow on every page; screenshots reviewed.)*
- [x] Lighthouse Accessibility ≥ 90 on the three main pages. *(100 / 100 / 100.)*
- [x] Full flow works with keyboard only. *(Login → skip link → quick add → complete (Space) → edit dialog (trap, Esc, focus back) → delete with confirm → detail → bell → logout; also via the mobile Main menu.)*
- [x] Pages load in < 2 s with 1,000 tasks. *(Task list < 1 s, dashboard < 0.4 s.)*
- [ ] Chrome and Edge smoke tests pass. *(Chrome ✅. Edge: waiting for the client's own check, as agreed on 2026-11-12.)*
