# Implementation Plan — Index

**Based on:** [BRD.md](../BRD.md) v1.4 and [ARCHITECTURE.md](../ARCHITECTURE.md) v1.0
**Execution rule:** Tasks run **strictly in sequential order**. A task starts only when every task it depends on is **Done** and its "Definition of Done" checklist is fully ticked.

## Task Order

| # | Task | Depends on | Layer | Status |
|---|---|---|---|---|
| 01 | [Environment setup](01-environment-setup.md) | — | Setup | ✅ Done |
| 02 | [Backend solution scaffold & cross-cutting setup](02-backend-scaffold.md) | 01 | Backend | ✅ Done |
| 03 | [Database: entities, DbContext, migrations](03-database-entities.md) | 02 | Backend | ✅ Done |
| 04 | [Authentication API](04-auth-api.md) | 03 | Backend | ✅ Done |
| 05 | [Category API](05-category-api.md) | 04 | Backend | ✅ Done |
| 06 | [Task API](06-task-api.md) | 05 | Backend | ✅ Done |
| 07 | [SubTask API](07-subtask-api.md) | 06 | Backend | ✅ Done |
| 08 | [Dashboard & Notifications API](08-dashboard-notifications-api.md) | 07 | Backend | ✅ Done |
| 09 | [Backend test coverage gate](09-backend-test-coverage.md) | 08 | Backend | ✅ Done |
| 10 | [Angular scaffold & core setup](10-frontend-scaffold.md) | 09 | Frontend | ✅ Done |
| 11 | [Frontend: Login & Register](11-frontend-auth.md) | 10 | Frontend | ✅ Done |
| 12 | [Frontend: Categories](12-frontend-categories.md) | 11 | Frontend | ✅ Done |
| 13 | [Frontend: Tasks](13-frontend-tasks.md) | 12 | Frontend | ✅ Done |
| 14 | [Frontend: SubTasks / Checklist](14-frontend-subtasks.md) | 13 | Frontend | ✅ Done |
| 15 | [Frontend: Dashboard & Notification bell](15-frontend-dashboard-notifications.md) | 14 | Frontend | ✅ Done |
| 16 | [Responsive & accessibility polish](16-responsive-accessibility.md) | 15 | Frontend | ✅ Done |
| 17 | [Database backup & restore](17-database-backup.md) | 16 | Ops | ✅ Done |
| 18 | [Final QA & BRD acceptance](18-final-qa-acceptance.md) | 17 | QA | ✅ Done |
| 19 | [Azure deployment](19-azure-deployment.md) (BRD v1.3) | 18 | Ops | ✅ Done |
| 20 | [Square buttons with rounded corners](20-square-buttons.md) (BRD v1.4, issue #1) | 19 | Frontend | ✅ Done |

## Dependency Chain

```
01 Env ─► 02 Scaffold ─► 03 Database ─► 04 Auth API ─► 05 Category API ─► 06 Task API
                                                                              │
     ┌────────────────────────────────────────────────────────────────────────┘
     ▼
07 SubTask API ─► 08 Dashboard/Notif API ─► 09 Test gate ─► 10 Angular scaffold ─► 11 Auth UI
                                                                                       │
     ┌─────────────────────────────────────────────────────────────────────────────────┘
     ▼
12 Categories UI ─► 13 Tasks UI ─► 14 SubTasks UI ─► 15 Dashboard/Bell UI ─► 16 Polish
                                                                                │
                                        18 Final QA ◄── 17 Backup ◄─────────────┘
         │
         ▼
19 Azure deployment ─► 20 Square buttons
```

## Why this order

- **Backend before frontend:** each UI screen needs a working, Swagger-tested API.
- **Auth (04) before every feature API:** all feature APIs need `UserId` from the JWT (AD-5).
- **Category (05) before Task (06):** a task has an optional `CategoryId` FK, and FR-16 (move tasks to Uncategorized) is tested in 06.
- **Task (06) before SubTask (07):** subtasks belong to a task.
- **Dashboard/Notifications (08) last on the backend:** they only read data created by 05–07.
- **Categories UI (12) before Tasks UI (13):** the task form needs the category dropdown.
- **Backup (17) after the app is complete:** the database schema is final by then.

## How to use these files

Run **`/start-process`** to start or resume. It follows these steps automatically. `/start-process status` only shows progress.

**Status values:** ☐ Not started · 🔄 In progress · 🟡 Awaiting merge · ✅ Done

Every task follows the workflow in [GUARDRAILS.md §1](../GUARDRAILS.md):

1. Create branch `task/NN-<name>` from the latest `main`.
2. Work through the task's **Steps** checklist, ticking `[x]` as you go, and tick every item in **Definition of Done**.
3. Run **`/task-review NN`**. The report is saved to `reviews/task-NN-review.md`.
4. **FAIL:** fix the listed issues and run the review again. Do not move on.
5. **PASS:** stop and ask the client to approve the merge.
6. After the client's "yes": merge into `main`, set the Status column here to ✅ Done, then start the next task.
