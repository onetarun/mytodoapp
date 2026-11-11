# Task 11 — Frontend: Login & Register

**Depends on:** 10
**References:** FR-1, FR-2, FR-5, NFR-8, NFR-11, NFR-23 · US-1
**Goal:** Users can register, log in and log out from the UI, with clear validation and error messages.

## Steps

### Register page (`/register`)
- [x] Reactive form: Name (required, max 100), Email (required, email), Password (required, min 8, at least one letter and one digit), Confirm password (must match).
- [x] Show inline field errors after touch/submit; disable the button while submitting.
- [x] Show the password rule hint under the field ("Min 8 characters, with a letter and a number").
- [x] On success: store the session (AuthService) and go to `/dashboard`.
- [x] On `400`: show server messages (e.g. "Email already registered").
- [x] Link to Login.

### Login page (`/login`)
- [x] Reactive form: Email, Password.
- [x] On success: go to `/dashboard` (or the originally requested URL).
- [x] On `401`: "Invalid email or password".
- [x] On `423`: "Account locked after too many attempts. Try again in 15 minutes." (NFR-11)
- [x] On `429`: "Too many attempts, please wait a minute."
- [x] Link to Register.

### Logout
- [x] Header Logout button: clear `sessionStorage`, reset state, go to `/login`. (FR-5)

### Accessibility
- [x] Every input has a `<label>`; errors are linked with `aria-describedby`; Enter submits the form; focus moves to the first invalid field on submit.

## Definition of Done

- [x] New user can register and lands on the dashboard; Work and Personal categories exist (check via API).
- [x] Logout → refresh → still logged out; login works again.
- [x] Wrong password 5 times shows the lockout message.
- [x] Closing the browser tab and reopening requires login again (sessionStorage).
- [x] Forms are fully usable with the keyboard only.

## Notes

- Both forms use Angular Material form fields. `<mat-label>` renders a real `<label for>`, and `<mat-hint>` / `<mat-error>` are linked to the input through `aria-describedby` automatically. Form-level messages use `role="alert"`.
- Shared helpers:
  - `shared/validators/password-rules.validator.ts`: a letter and a digit, the same rule as the API.
  - `shared/validators/matches-control.validator.ts`: confirm password; it is re-checked when the password changes.
  - `shared/forms/apply-server-errors.ts`: ProblemDetails `errors` → `{ server: message }` on the matching control.
  - `shared/forms/focus-first-invalid.ts`: uses the control state, so it works right after server errors.
- Field limits live in `features/auth/auth-form-rules.ts`. Login messages are in `LOGIN_MESSAGES`.
- **Login errors:**
  - 401 → "Invalid email or password"
  - 423 → lockout message
  - 429 → "Too many attempts, please wait a minute."
  - 400 → the server message.
  - `errorInterceptor` (Task 10) no longer toasts 429 for the login call, so the message is not shown twice.
- **Return URL:** only same-app paths are followed (`/…`, not `//…`, `/\…` or `/login…`). Anything else goes to `/dashboard`, so a crafted link cannot redirect off-site.
- **Logout:** the header button from Task 10 already clears sessionStorage and the state and goes to `/login`. It was verified here with the keyboard.
- **Prettier:** `.prettierrc` now has `"endOfLine": "auto"`. With `core.autocrlf=true`, Windows checkouts have CRLF files, which would otherwise fail `prettier --check`.

### Verification (2026-11-11, `ng serve` + API `https` profile, browser pane, keyboard input)

| Check | Result |
|---|---|
| Lint / Prettier / build | all files pass linting; Prettier clean (also on CRLF files); build succeeds with no warnings, initial 472 kB |
| Empty register submit | four inline errors; focus moves to Name; Name's `aria-describedby` points to its `mat-error`; `<label for>` present |
| Weak password + mismatch (Enter submits) | "Password must contain at least one number.", "Passwords do not match."; focus on Password |
| Valid register (keyboard only) | lands on `/dashboard`, header shows the name; `GET /api/v1/categories` → Personal, Work |
| Duplicate email | "An account with this email already exists." under Email; focus moves to Email |
| Logout (Enter on the focused button) → reload `/categories` | `/login?returnUrl=%2Fcategories`, session removed |
| Wrong password | inline "Invalid email or password", no toast |
| Correct password | goes back to `/categories` (returnUrl) |
| 5 wrong passwords | attempts 1–4 "Invalid email or password", attempt 5 and the 6th (correct password) "Account locked after too many attempts. Try again in 15 minutes." |
| More attempts in the same minute | "Too many attempts, please wait a minute." (429), no duplicate toast |
| New tab while logged in elsewhere | no session in the new tab → `/login?returnUrl=%2Fdashboard` |
| Console | no app errors. Only the browser's network lines for the deliberately failed 401/423/429/400 requests |
