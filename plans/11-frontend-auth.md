# Task 11 — Frontend: Login & Register

**Depends on:** 10
**References:** FR-1, FR-2, FR-5, NFR-8, NFR-11, NFR-23 · US-1
**Goal:** Users can register, log in and log out from the UI, with clear validation and error messages.

## Steps

### Register page (`/register`)
- [ ] Reactive form: Name (required, max 100), Email (required, email), Password (required, min 8, at least one letter and one digit), Confirm password (must match).
- [ ] Show inline field errors after touch/submit; disable the button while submitting.
- [ ] Show the password rule hint under the field ("Min 8 characters, with a letter and a number").
- [ ] On success: store the session (AuthService) and go to `/dashboard`.
- [ ] On `400`: show server messages (e.g. "Email already registered").
- [ ] Link to Login.

### Login page (`/login`)
- [ ] Reactive form: Email, Password.
- [ ] On success: go to `/dashboard` (or the originally requested URL).
- [ ] On `401`: "Invalid email or password".
- [ ] On `423`: "Account locked after too many attempts. Try again in 15 minutes." (NFR-11)
- [ ] On `429`: "Too many attempts, please wait a minute."
- [ ] Link to Register.

### Logout
- [ ] Header Logout button: clear `sessionStorage`, reset state, go to `/login`. (FR-5)

### Accessibility
- [ ] Every input has a `<label>`; errors are linked with `aria-describedby`; Enter submits the form; focus moves to the first invalid field on submit.

## Definition of Done

- [ ] New user can register and lands on the dashboard; Work and Personal categories exist (check via API).
- [ ] Logout → refresh → still logged out; login works again.
- [ ] Wrong password 5 times shows the lockout message.
- [ ] Closing the browser tab and reopening requires login again (sessionStorage).
- [ ] Forms are fully usable with the keyboard only.
