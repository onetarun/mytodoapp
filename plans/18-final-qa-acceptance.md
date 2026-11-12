# Task 18 — Final QA & BRD Acceptance

**Depends on:** 17
**References:** BRD §4, §5, §7, §10
**Goal:** Every BRD requirement is verified, issues are fixed, and the client signs off on v1.

## Steps

### Functional checklist (BRD §4)

| ID | Check | Pass |
|---|---|---|
| FR-1 | Register with name, email, password | ✅ API 201 + token; duplicate email 400; UI register → dashboard |
| FR-2 | Login returns a JWT and opens the dashboard | ✅ API 200 with JWT; UI login → dashboard (or the guard's `returnUrl`) |
| FR-3 | Passwords stored as hashes in the DB | ✅ `AspNetUsers.PasswordHash` = `AQAAAAIA…` (Identity PBKDF2), never the plain text |
| FR-4 | API without a token → 401; user A can't see user B's data | ✅ 4 endpoints without token + tampered token → 401; user B on user A's ids → 404 |
| FR-5 | Logout clears the session | ✅ UI logout → /login, sessionStorage cleared, /tasks redirects to /login |
| FR-6 | Create task with title, description, due date, priority, category | ✅ API all fields round-trip; UI with due date, High, category "Home" |
| FR-7 | Edit every task field | ✅ API edits and clears every field |
| FR-8 | Delete asks for confirmation | ✅ UI confirm dialog "Delete '…'?"; Cancel keeps the task; API delete → 204 then 404 |
| FR-9 | Checkbox completes / reopens a task | ✅ API complete / reopen (`completedAt` set / cleared); UI checkbox |
| FR-10 | List shows title, priority, category, due date, status | ✅ list rows carry title, priority, category, due date, completed, overdue, progress |
| FR-10a | Status filter All / Pending / Completed stays applied | ✅ API all 6 / pending 5 / completed 1; UI `?status=completed` survives a reload |
| FR-11 | Add / edit / delete subtasks | ✅ add 4 (sortOrder 1–4), edit, delete |
| FR-12 | Tick / untick subtasks | ✅ tick / untick |
| FR-13 | Progress "x/y" shown | ✅ API 2/3 after a delete; UI "1 of 3 done" and list "1/3" |
| FR-14 | New user gets Work and Personal | ✅ new users get Work and Personal (API + UI) |
| FR-15 | Create / rename / delete categories | ✅ create, duplicate (case-insensitive) → 400, rename, delete |
| FR-16 | Deleting a category makes its tasks Uncategorized | ✅ deleting "Shopping" → its 3 tasks have `categoryId` null |
| FR-17 | Dashboard counts are correct | ✅ counts equal the task list (API and UI: 4 / 1 / 3 / 1 / 2) |
| FR-18 | Upcoming 7-day list is correct | ✅ upcoming = pending tasks due in the next 7 days only |
| FR-19 | Bell shows overdue + due-in-24h count | ✅ API count 2 (1 overdue + 1 due soon); UI "Notifications, 2 items" |
| FR-20 | Bell list links to tasks | ✅ each item links to `/tasks/{id}` |
| FR-21 | Overdue tasks red + icon | ✅ `isOverdue` only on past-due pending tasks; UI red border + warning icon + "Overdue" |
| FR-22 | Notifications refresh every 5 min and on load | ✅ load + every 5 min, refresh after changes, stops on logout (Task 15 review) |

### Non-functional checklist (BRD §5)

| ID | Check | Pass |
|---|---|---|
| NFR-1/2 | API < 500 ms, pages < 2 s with 1,000 tasks | ✅ 100k tasks in DB: API p95 ≤ 83 ms (GET /tasks 1,000 rows), others ≤ 44 ms; task list 0.8–1.25 s, dashboard 0.35–0.6 s (production build) |
| NFR-3/4 | Indexes exist; 100 users × 1,000 tasks seeded test is fine | ✅ indexes `TodoTasks(UserId, IsDeleted, IsCompleted)`, `(UserId, DueDate)`, `Categories(UserId)`, `SubTasks(TodoTaskId)`; 100 users × 1,000 tasks seeded through the API in 251 s, 0 failures |
| NFR-5/6 | Daily backup runs; restore tested | ✅ Task 17: scheduled task daily 02:00, verified `.bak`, 7-day cleanup, test restore 2026-11-12 |
| NFR-7 | Deletes are soft (rows remain with `IsDeleted = 1`) | ✅ deleted task / subtask / category rows have `IsDeleted = 1` and `DeletedAt` |
| NFR-8/9 | Password policy enforced in UI and API; hashed | ✅ API: no digit / no letter / 7 chars → 400; UI form rejects "abcdefgh"; hashes as FR-3 |
| NFR-10 | Session expires after 60 min → redirected to login | ✅ JWT lifetime 60 min; with a 1-min token: UI → /login at 60 s with "session has expired" toast; API → 401 after expiry + 1-min skew |
| NFR-11 | 5 wrong passwords → 15-min lockout | ✅ 5th wrong password → 423 "Account locked."; correct password → 423; `LockoutEnd` = now + 15 min; unknown email → same 401 as a wrong password |
| NFR-12 | HTTPS only | ✅ http://localhost:5187 → 307 to https; HSTS outside Development |
| NFR-13 | Invalid input rejected; `<script>` in a title is shown as text, not run | ✅ empty / 201-char title, 2001-char description, bad priority, bad time zone → 400 with field errors; `<script>` title stored as-is and shown as text (no script, no alert) |
| NFR-14 | API rejects calls from another origin (CORS) | ✅ preflight from localhost:4200 → allowed; from evil.example → no `Access-Control-Allow-Origin` |
| NFR-15 | Rate limits return 429 | ✅ 11th login in a minute → 429 |
| NFR-16 | Ownership isolation (try user B's ids with user A's token) | ✅ 9 kinds of calls with B's token on A's ids → 404 (same as a missing id); A's data unchanged; `userId` in a body is ignored |
| NFR-17/18 | Errors logged to file; users see friendly messages, no stack traces | ✅ daily log file, 30 days kept, no passwords or tokens in it; malformed JSON / 404 → ProblemDetails without internals; 500 handler covered by `GlobalExceptionHandlerTests` |
| NFR-19–22 | Minimal UI, responsive 360–1920 px, ≤ 2 clicks to add, English | ✅ Task 16: 360–1920 px without overflow; title + Enter adds a task; English only |
| NFR-23/24 | Keyboard navigation, labels, contrast, status not by color alone | ✅ Task 16: keyboard-only flow, Lighthouse Accessibility 100 on all three pages, text/icon for every status |
| NFR-25 | Chrome and Edge latest | ✅ Chrome 157 (all flows) / ⚠️ Edge 157: client check pending |
| NFR-26 | Backend tests pass, coverage ≥ 60% | ✅ 137/137 tests pass; `TodoApp.Api.Services` lines 802/802 = 100 %, branches 92.4 % |
| NFR-27 | Swagger documents all endpoints | ✅ Swagger lists all 18 ARCHITECTURE endpoints + health (19 operations) |
| NFR-28 | Layered structure; no secrets in code or git | ✅ no `AppDbContext` in Controllers; secrets only in user secrets; no secret values in git history |

### User stories (BRD §7)
- [x] Walk through US-1 to US-7 as a brand-new user, end to end. *(Headless Chrome 157 on the production build, new user `e2e-…@example.test`: register (weak password rejected first) → logout → login → own category "Home" → quick add + task with due date / High / Home → overdue task → `<script>` title shown as text → overdue row red + bell "2 items" with links → complete + `?status=completed` after reload → delete confirm (Cancel) → checklist "1 of 3 done" / list "1/3" → dashboard 4 / 1 / 3 / 1 / 2 with the upcoming task.)*

### Wrap-up
- [x] Log every failed check as an issue, fix it, and re-test. *(See "Issues found" below.)*
- [x] Write `README.md`: prerequisites, how to set user secrets, run migrations, start the API and Angular app, run tests.
- [ ] Demo to the client. *(Client action.)*
- [ ] Client ticks **Approved** in BRD §10. *(Client action.)*

## Definition of Done

- [ ] Every FR and NFR above is ticked. *(All ✅ except the Edge half of NFR-25, which the client checks.)*
- [x] No open critical or high issues.
- [x] README is complete and a fresh setup works by following it. *(Fresh `git clone` in a scratch folder, new database `TodoAppDb_ReadmeCheck`: build 0 warnings, format, 137 tests, `dotnet ef database update` (11 tables), `npm ci`, lint, format check, production build, API from the clone → register 201 with Work + Personal. Temporary database dropped afterwards.)*
- [ ] Client sign-off recorded in the BRD.

## Issues found

| # | Check | Severity | Issue | Fix | Re-test |
|---|---|---|---|---|---|
| 1 | README fresh setup | Medium | On a fresh clone, `dotnet ef database update` failed with `project.assets.json not found` because the packages were not restored yet. | README §3 now runs `dotnet build` first and explains why. | ✅ migrations created the database in the clone |

Checks that first showed FAIL because of the test script, not the app (re-tested and passing): NFR-16 (the PUT body used a category that no longer existed, so validation answered 400 before ownership; with a valid body every call → 404), NFR-11 (the account locks on the 5th wrong password, so that call already returns 423, as the BRD requires), NFR-27 (the script expected ≥ 20 operations; the API has 19 = 18 ARCHITECTURE endpoints + health), US-1 login (after the guard redirect, login correctly returns to `returnUrl` `/tasks`, not the dashboard).

## Test data left in `TodoAppDb`

Users created by QA (all `@example.test`): `qa-*`, `e2e-*`, `load-001` … `load-100` (1,000 tasks each), plus the users from Tasks 14–16. Remove them before real use if a clean database is wanted (this is a destructive action and needs the client's approval).
