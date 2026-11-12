# Business Requirements Document (BRD) — Todo Application

**Version:** 1.4 (button shape added — NFR-22a)
**Date:** 2026-11-12
**Status:** v1 approved 2026-11-12; v1.3 hosting scope approved by the client on 2026-11-12; v1.4 button shape (GitHub issue #1) approved by the client on 2026-11-12

---

## 1. Vision

A **simple, fast task-tracking web application for individual users**. A user can quickly add, organize and complete personal tasks through a clean, minimal UI, with reminders and a dashboard that keep them on top of deadlines.

## 2. Target Users

| Item | Decision |
|---|---|
| Primary user | Individual users (students, professionals) |
| Multi-user | Yes. Each user has their own account and sees only their own tasks |
| Team/collaboration | Out of scope for v1 |

## 3. Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Angular (SPA) |
| Backend | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | SQL Server (local instance for development); Azure SQL Database in Azure |
| Hosting | Microsoft Azure: App Service (Linux) for the API and the Angular build, Azure SQL Database (NFR-29) |
| Authentication | JWT token-based, with hashed passwords |
| Platform | Web (desktop and mobile browsers, responsive) |

## 4. Functional Requirements (MVP Scope)

### 4.1 Authentication
- **FR-1:** A user can register with name, email and password.
- **FR-2:** A user can log in with email and password and receives a JWT.
- **FR-3:** Passwords are stored hashed, never as plain text.
- **FR-4:** All task, category and dashboard APIs require a valid JWT, and a user can access only their own data.
- **FR-5:** A user can log out, which clears the token on the client.

### 4.2 Task Management
- **FR-6:** A user can create a task with these fields:
  - Title (required, max 200 chars)
  - Description (optional)
  - Due date (optional, date + time)
  - Priority: High / Medium / Low (default: Medium)
  - Category (optional, chosen from the user's categories)
- **FR-7:** A user can edit any field of their task.
- **FR-8:** A user can delete a task after a confirmation prompt.
- **FR-9:** A user can mark a task complete or incomplete with a checkbox.
- **FR-10:** The task list shows title, priority badge, category, due date and status.
- **FR-10a:** A **Status filter** above the task list with three options: **All / Pending / Completed** (default: All). The selected filter stays applied while the user adds, edits or completes tasks.

### 4.3 Subtasks / Checklist
- **FR-11:** A user can add, edit and delete subtasks (checklist items) inside a task.
- **FR-12:** Each subtask can be marked done or undone.
- **FR-13:** The parent task shows checklist progress, for example "2/5".

### 4.4 Categories
- **FR-14:** New users get default categories: **Work** and **Personal**.
- **FR-15:** A user can create, rename and delete their own categories.
- **FR-16:** When a category is deleted, its tasks become "Uncategorized" and are not deleted.

### 4.5 Dashboard / Stats
- **FR-17:** The dashboard shows counts for **Total, Completed, Pending, Overdue** and **Due Today**.
- **FR-18:** The dashboard shows a short list of upcoming tasks (next 7 days).

### 4.6 Reminders & Overdue Handling
- **FR-19:** In-app notification bell in the header with a count of overdue and due-soon (within 24 hours) tasks.
- **FR-20:** Clicking the bell lists those tasks with links to open them.
- **FR-21:** Overdue tasks (past due date and not completed) are **highlighted in red** in the list.
- **FR-22:** Notifications refresh on page load and periodically (for example every 5 minutes) while the app is open.

## 5. Non-Functional Requirements

### 5.1 Performance
- **NFR-1:** 95% of API calls respond in **under 500 ms**.
- **NFR-2:** Pages (task list, dashboard) fully load in **under 2 seconds**.

### 5.2 Scalability / Capacity
- **NFR-3:** Supports about **100 registered users** with up to **1,000 tasks per user** and no performance drop.
- **NFR-4:** Database indexes on `UserId`, `DueDate`, `IsCompleted` and `IsDeleted` for fast queries.

### 5.3 Availability, Backup & Data Retention
- **NFR-5:** **Daily automatic SQL Server backup** (SQL Server Agent job or scheduled script). The last **7 days** of backups are kept.
- **NFR-6:** Restore from backup is documented and tested at least once.
- **NFR-5a:** In Azure, NFR-5 is met by Azure SQL Database automated backups with **7 days** of point-in-time restore. The local Task Scheduler backup (Task 17) stays for the local database.
- **NFR-7:** **Soft delete:** deleting a task, subtask or category sets `IsDeleted = true` and `DeletedAt`. The data is hidden from users but stays in the DB. This is applied through an EF Core global query filter. There is no Trash/restore UI in v1.

### 5.4 Security
- **NFR-8:** Password policy: **minimum 8 characters, at least one letter and one number**. The UI and the API both enforce it.
- **NFR-9:** Passwords are hashed with a strong algorithm (ASP.NET Core Identity PasswordHasher / BCrypt).
- **NFR-10:** JWT access token valid for **60 minutes**. After it expires, the user is redirected to login. There is no refresh token in v1.
- **NFR-11:** **Account lockout:** 5 consecutive failed logins lock the account for 15 minutes.
- **NFR-12:** **HTTPS** is used for all communication.
- **NFR-13:** **Input validation** on the server for every request. SQL injection is prevented through EF Core parameterized queries, and XSS through Angular's built-in sanitization.
- **NFR-14:** **CORS** allows only the Angular app's origin.
- **NFR-15:** **Rate limiting** using ASP.NET Core's built-in middleware. Example limits: login 10 requests/min per IP, other APIs 100 requests/min per user.
- **NFR-16:** Users can only access their own data. Every query is scoped by `UserId` taken from the JWT.

### 5.5 Logging & Monitoring
- **NFR-17:** **Error logging to file** using Serilog, with rolling daily log files kept for 30 days.
- **NFR-18:** Users see friendly error messages. Stack traces and internal details are never exposed in API responses.

### 5.5a Hosting & Deployment (Azure)
- **NFR-29:** The app is hosted on **Azure App Service (Linux, .NET 10)**. The Angular production build is served by the API from the same URL (`https://<app>.azurewebsites.net`), so the browser calls `/api/v1` on the same origin.
- **NFR-30:** The database is **Azure SQL Database**. Schema changes are applied with the existing EF Core migrations during deployment.
- **NFR-31:** Secrets (SQL connection string, JWT key) live only in App Service application settings, never in git. HTTPS-only is on, and logs are written to the App Service `/home/LogFiles` folder (NFR-17).
- Deployment is a manual script run by the client after `az login` (no CI/CD in v1; GitHub Actions is Phase 2).

### 5.6 Usability & UI
- **NFR-19:** Minimal and clean list-based design.
- **NFR-20:** **Responsive** from 360 px (mobile) to 1920 px (desktop).
- **NFR-21:** A task can be added in 2 clicks or fewer from the main screen.
- **NFR-22:** UI language is **English only**.
- **NFR-22a:** Text buttons (text, filled, outlined) are **rectangles with rounded corners (8 px radius)**, not pill/oval shapes. Icon-only buttons stay circular. Set once globally in `styles.scss` (GitHub issue #1).

### 5.7 Accessibility
- **NFR-23:** **Basic accessibility:** full keyboard navigation, labels on all form fields, sufficient color contrast, and visible focus states.
- **NFR-24:** Status is never shown by color alone. Overdue tasks get a red highlight **plus** a warning icon or text.

### 5.8 Compatibility
- **NFR-25:** Supported browsers: **Google Chrome and Microsoft Edge, latest 2 versions**. Other browsers are not officially tested.

### 5.9 Maintainability & Quality
- **NFR-26:** **Backend unit tests** with xUnit, with at least **60% code coverage** on business logic and services.
- **NFR-27:** **Swagger / OpenAPI** documentation for all API endpoints, available in the development environment.
- **NFR-28:** Layered backend structure (Controllers → Services → Data), with configuration (connection string, JWT secret) in `appsettings.json` or user secrets, never hard-coded.

## 6. Data Model (High Level)

| Entity | Key Fields |
|---|---|
| **User** | Id, Name, Email (unique), PasswordHash, FailedLoginCount, LockoutEnd, CreatedAt |
| **Category** | Id, UserId (FK), Name, IsDeleted, DeletedAt |
| **TodoTask** | Id, UserId (FK), CategoryId (FK, nullable), Title, Description, DueDate, Priority, IsCompleted, CompletedAt, CreatedAt, UpdatedAt, IsDeleted, DeletedAt |
| **SubTask** | Id, TodoTaskId (FK), Title, IsCompleted, SortOrder, IsDeleted, DeletedAt |

## 7. User Stories

| ID | As a… | I want to… | So that… |
|---|---|---|---|
| US-1 | new user | register and log in | my tasks are private to me |
| US-2 | user | quickly add a task with a due date and priority | I can capture work fast |
| US-3 | user | tick a task as done | I can track my progress |
| US-4 | user | break a task into checklist items | big tasks feel manageable |
| US-5 | user | organize tasks into my own categories | I can separate work and personal life |
| US-6 | user | see overdue tasks in red and in the bell | I don't miss deadlines |
| US-7 | user | see a dashboard summary | I know my status at a glance |

## 8. Out of Scope for v1 (Phase 2 Backlog)

- Search, advanced filters (priority, category, date) and sort *(recommended as the first Phase 2 item; the basic Status filter is already in the MVP)*
- Forgot password via email reset
- Profile edit / change password
- "Remember me"
- Browser push and email reminders
- Dark mode
- Trash / restore UI for soft-deleted tasks
- Refresh tokens (stay logged in longer than 60 minutes)
- Login audit and task change history
- Frontend unit tests and end-to-end tests
- Firefox / Safari support, multi-language (Hindi) UI
- Kanban board view
- Team sharing and task assignment
- Native mobile apps
- CI/CD auto-deploy to Azure (GitHub Actions), custom domain

## 9. Assumptions & Open Points

1. SQL Server runs locally (Express or Developer edition is fine).
2. **Resolved (v1.3):** Development runs on a local machine; production hosting is Microsoft Azure (NFR-29 to NFR-31). The Azure subscription and its cost are owned by the client.
3. Times are stored in UTC and shown in the user's local time zone.
4. **Resolved:** The client approved adding a basic Status filter (All / Pending / Completed) to the MVP (see FR-10a).
5. Category deletion is a soft delete. Its tasks are moved to "Uncategorized" as per FR-16.
6. Since soft-deleted data stays in the DB, a permanent purge policy (for example, hard-delete after 1 year) can be decided later if needed.

## 10. Sign-off

| Role | Name | Approval |
|---|---|---|
| Client | Client | ☑ Approved — 2026-11-12 (v1 accepted after Task 18 QA; Edge smoke test and demo left to the client) |
| Business Analyst | Claude | ✔ Prepared |
