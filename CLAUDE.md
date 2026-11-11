# Todo Application — Project Rules for Claude

Read these before doing any work in this project:

- [BRD.md](BRD.md): what to build (functional and non-functional requirements)
- [ARCHITECTURE.md](ARCHITECTURE.md): how to build it (AD-1 to AD-5, endpoints, folder structure)
- [GUARDRAILS.md](GUARDRAILS.md): rules that must always be followed (G-PROC, G-SCOPE, G-GIT, G-DEP, G-SEC, G-CODE, G-TEST, G-DB, G-DESTRUCT)
- [plans/00-INDEX.md](plans/00-INDEX.md): the ordered task list and each task's status

## Hard rules (summary — GUARDRAILS.md is the full source)

1. **Do not start implementation until the client says "yes".**
2. Work on **one task at a time**, in the order of `plans/00-INDEX.md`, on branch `task/NN-<name>`.
3. When a task is finished, run **`/task-review NN`**.
   - **FAIL:** fix the issues and review again. Never move to the next task.
   - **PASS:** stop and ask the client to approve the merge. Merge into `main` only after an explicit "yes", then mark the task ✅ in the index.
4. Build nothing outside BRD / ARCHITECTURE / the task file without the client's approval. Update the BRD first when scope changes.
5. Add only packages from GUARDRAILS Appendix A; ask before adding anything else.
6. Never: commit secrets, log passwords/tokens, take `UserId` from the request, use raw SQL, use `innerHTML`/`bypassSecurityTrust*`, edit an applied migration, or push to a remote.
7. Ask before any destructive action (DB drop/reset, `git reset --hard`, force push, deleting files not created in this task).

## Commands

| Command | Purpose |
|---|---|
| `/start-process` | Start or resume the process: picks the next task, implements it, runs the review, and stops for merge approval. Typing it counts as the client's "yes" to implement. |
| `/start-process status` | Show progress only; changes nothing. |
| `/task-review NN` | Review task NN against its Definition of Done and the guardrails, and write `reviews/task-NN-review.md`. |

## Git bootstrap (first run only)

The folder is not a git repository yet. The first `/start-process` run does `git init`, adds `.gitignore`, makes the first commit on `main` with the existing documents (`[Task 00] Add BRD, architecture, guardrails and plans`), then creates `task/01-environment-setup`.

## Communication

The client communicates in Hinglish. Reply in Hinglish; keep code, file contents and documents in English.
