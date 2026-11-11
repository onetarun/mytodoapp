# Task 18 — Final QA & BRD Acceptance

**Depends on:** 17
**References:** BRD §4, §5, §7, §10
**Goal:** Every BRD requirement is verified, issues are fixed, and the client signs off on v1.

## Steps

### Functional checklist (BRD §4)

| ID | Check | Pass |
|---|---|---|
| FR-1 | Register with name, email, password | ☐ |
| FR-2 | Login returns a JWT and opens the dashboard | ☐ |
| FR-3 | Passwords stored as hashes in the DB | ☐ |
| FR-4 | API without a token → 401; user A can't see user B's data | ☐ |
| FR-5 | Logout clears the session | ☐ |
| FR-6 | Create task with title, description, due date, priority, category | ☐ |
| FR-7 | Edit every task field | ☐ |
| FR-8 | Delete asks for confirmation | ☐ |
| FR-9 | Checkbox completes / reopens a task | ☐ |
| FR-10 | List shows title, priority, category, due date, status | ☐ |
| FR-10a | Status filter All / Pending / Completed stays applied | ☐ |
| FR-11 | Add / edit / delete subtasks | ☐ |
| FR-12 | Tick / untick subtasks | ☐ |
| FR-13 | Progress "x/y" shown | ☐ |
| FR-14 | New user gets Work and Personal | ☐ |
| FR-15 | Create / rename / delete categories | ☐ |
| FR-16 | Deleting a category makes its tasks Uncategorized | ☐ |
| FR-17 | Dashboard counts are correct | ☐ |
| FR-18 | Upcoming 7-day list is correct | ☐ |
| FR-19 | Bell shows overdue + due-in-24h count | ☐ |
| FR-20 | Bell list links to tasks | ☐ |
| FR-21 | Overdue tasks red + icon | ☐ |
| FR-22 | Notifications refresh every 5 min and on load | ☐ |

### Non-functional checklist (BRD §5)

| ID | Check | Pass |
|---|---|---|
| NFR-1/2 | API < 500 ms, pages < 2 s with 1,000 tasks | ☐ |
| NFR-3/4 | Indexes exist; 100 users × 1,000 tasks seeded test is fine | ☐ |
| NFR-5/6 | Daily backup runs; restore tested | ☐ |
| NFR-7 | Deletes are soft (rows remain with `IsDeleted = 1`) | ☐ |
| NFR-8/9 | Password policy enforced in UI and API; hashed | ☐ |
| NFR-10 | Session expires after 60 min → redirected to login | ☐ |
| NFR-11 | 5 wrong passwords → 15-min lockout | ☐ |
| NFR-12 | HTTPS only | ☐ |
| NFR-13 | Invalid input rejected; `<script>` in a title is shown as text, not run | ☐ |
| NFR-14 | API rejects calls from another origin (CORS) | ☐ |
| NFR-15 | Rate limits return 429 | ☐ |
| NFR-16 | Ownership isolation (try user B's ids with user A's token) | ☐ |
| NFR-17/18 | Errors logged to file; users see friendly messages, no stack traces | ☐ |
| NFR-19–22 | Minimal UI, responsive 360–1920 px, ≤ 2 clicks to add, English | ☐ |
| NFR-23/24 | Keyboard navigation, labels, contrast, status not by color alone | ☐ |
| NFR-25 | Chrome and Edge latest | ☐ |
| NFR-26 | Backend tests pass, coverage ≥ 60% | ☐ |
| NFR-27 | Swagger documents all endpoints | ☐ |
| NFR-28 | Layered structure; no secrets in code or git | ☐ |

### User stories (BRD §7)
- [ ] Walk through US-1 to US-7 as a brand-new user, end to end.

### Wrap-up
- [ ] Log every failed check as an issue, fix it, and re-test.
- [ ] Write `README.md`: prerequisites, how to set user secrets, run migrations, start the API and Angular app, run tests.
- [ ] Demo to the client.
- [ ] Client ticks **Approved** in BRD §10.

## Definition of Done

- [ ] Every FR and NFR above is ticked.
- [ ] No open critical or high issues.
- [ ] README is complete and a fresh setup works by following it.
- [ ] Client sign-off recorded in the BRD.
