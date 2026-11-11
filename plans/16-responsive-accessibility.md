# Task 16 — Responsive & Accessibility Polish

**Depends on:** 15
**References:** NFR-2, NFR-19 to NFR-25
**Goal:** The whole app looks right from 360 px to 1920 px, works with the keyboard, and meets the basic accessibility rules.

## Steps

### Responsive (NFR-20)
- [ ] Check every page at **360, 768, 1280 and 1920 px** (browser dev tools).
- [ ] Header collapses into a menu on small screens; the bell stays visible.
- [ ] Task rows wrap cleanly; actions stay reachable (≥ 44 px touch targets).
- [ ] Dashboard cards: 1 column on mobile, 2–3 on tablet, 5 on desktop.
- [ ] Dialogs fit the screen on mobile; no horizontal scrolling anywhere.

### Accessibility (NFR-23, NFR-24)
- [ ] Tab through every page: logical order, **visible focus ring**, no keyboard traps; dialogs trap focus and return it on close.
- [ ] All inputs have labels; icon-only buttons have `aria-label`.
- [ ] Color contrast ≥ 4.5:1 for text (check with Lighthouse or an axe browser extension).
- [ ] Overdue, priority and completed states are shown with text/icons, not color alone.
- [ ] Run **Lighthouse Accessibility** on Login, Dashboard and Tasks; fix issues until the score is ≥ 90.

### Performance (NFR-2)
- [ ] `ng build` (production) — check bundle sizes; confirm feature routes are lazy-loaded.
- [ ] With 1,000 tasks, the task list and dashboard load in < 2 s (Lighthouse or network tab).

### Browsers (NFR-25)
- [ ] Smoke test the full flow in the latest **Chrome** and **Edge**.

### UI consistency (NFR-19)
- [ ] Consistent spacing, fonts and buttons; minimal and clean look; English-only text (NFR-22).

## Definition of Done

- [ ] No layout breaks at 360 / 768 / 1280 / 1920 px.
- [ ] Lighthouse Accessibility ≥ 90 on the three main pages.
- [ ] Full flow works with keyboard only.
- [ ] Pages load in < 2 s with 1,000 tasks.
- [ ] Chrome and Edge smoke tests pass.
