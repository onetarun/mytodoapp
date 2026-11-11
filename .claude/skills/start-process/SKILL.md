---
name: start-process
description: Start or resume the Todo Application development process - picks the next task from plans/00-INDEX.md, creates its branch, implements it under GUARDRAILS.md, runs /task-review, and stops for the client's merge approval. Usage - /start-process (next task), /start-process NN (a specific task, only if it is next), /start-process status (show progress only).
argument-hint: "[NN | status]"
disable-model-invocation: true
---

# Start Process

The client typing `/start-process` **is** the client's "yes" to start implementation (G-PROC-2). It covers the task picked below and the tasks that follow after each approved merge. It never covers merges: every merge still needs its own explicit "yes" (G-PROC-5).

Reply to the client in Hinglish; write code, files and documents in English.

## 1. Load context

Read in full: `CLAUDE.md`, `GUARDRAILS.md`, `plans/00-INDEX.md`. Read `BRD.md` and `ARCHITECTURE.md` sections when a task references them.

## 2. Work out the current state

Arguments: `$ARGUMENTS`

- **`status`**: print a short table of every task with its status from the index, the current git branch, and the latest review verdict (from `reviews/`). Then stop. Do nothing else.
- **No argument**: the target is the first task in the index that is not ✅ Done.
- **`NN`**: the target is task NN, but only if it is that first not-done task. Otherwise refuse, explain G-PROC-1, and name the task that must come first.

Then check which situation applies, in this order:

| Situation | What to do |
|---|---|
| Not a git repository | Go to **3. Git bootstrap**, then continue. |
| Uncommitted changes on `main` | Stop. Show `git status` and ask the client what to do. Do not stash, reset or discard (G-DESTRUCT). |
| On `task/NN-...` and `reviews/task-NN-review.md` says **PASS** for the latest commit | The task is waiting for merge approval. Go to **7. Ask for merge**. |
| On `task/NN-...` with work in progress | Resume: continue at **5. Implement** from the first unticked step in the task file. |
| On `main`, clean, previous tasks ✅ | Go to **4. Start the task**. |

## 3. Git bootstrap (first run only)

1. `git init -b main`
2. Create `.gitignore` for .NET, Node and this project: `bin/`, `obj/`, `node_modules/`, `logs/`, `*.user`, `*.bak`, `.vs/`, `.angular/`, `dist/`, `coverage/`, `TestResults/`.
3. Commit the existing documents: `[Task 00] Add BRD, architecture, guardrails and plans`.

## 4. Start the task

1. Pre-flight checks:
   - Every earlier task is ✅ in the index (G-PROC-1).
   - The working tree is clean and you are on `main`.
   - The tools the task needs are available (e.g. `dotnet --version`, `node -v`, `ng version`, SQL Server reachable). If something is missing, stop and tell the client exactly what to install. **Never install system software (SDKs, SQL Server, Node) yourself without the client's explicit permission.**
2. Create the branch: `git checkout -b task/NN-<short-name>` (name from the task file name, G-GIT-1).
3. In `plans/00-INDEX.md`, set the task's status to `🔄 In progress`.
4. Tell the client in 2–3 lines: which task, its goal, and what will be built.

## 5. Implement

Work through the task file's **Steps** in order:

- Tick each step `[x]` in the task file as soon as it is done.
- Commit in small, logical pieces: `[Task NN] <imperative summary>` (G-GIT-2). Never push (G-GIT-4).
- Follow every guardrail while writing code, not only at review time:
  - **G-SCOPE:** if something outside the BRD, ARCHITECTURE or task file seems needed, **stop and ask** the client. If they approve, update the BRD first.
  - **G-DEP:** add only Appendix A packages; ask before any other package.
  - **G-SEC, G-CODE, G-TEST, G-DB:** apply them as you write.
  - **G-DESTRUCT:** ask before any destructive action.
- If you are blocked (G-PROC-7), stop and explain the blocker. Do not work around it.
- When all steps are done, tick the **Definition of Done** items you have verified, with the command or check you used.

## 6. Review loop

1. Run the **`task-review`** skill for task NN.
2. **FAIL:** fix every listed issue on the same branch, commit the fixes, and run the review again.
3. After **3 FAIL reviews in a row**, stop and show the client the remaining issues. Ask how to proceed instead of looping further.
4. **PASS:** go to step 7.

## 7. Ask for merge (stop here)

1. Set the task's status in the index to `🟡 Awaiting merge` and commit it on the branch.
2. Tell the client:
   - task NN is done and the review **PASSED**;
   - a short summary of what was built;
   - any **⚠️ NOT VERIFIED** items from the report, which the client should check themselves;
   - the report path `reviews/task-NN-review.md`.
3. Ask: "Task NN ko `main` mein merge karun?" Then **end your turn and wait**. Do not merge and do not start the next task.

## 8. After the client's answer

- **"yes" / approval:**
  1. `git checkout main` and `git merge --no-ff task/NN-<short-name> -m "[Task NN] Merge <task name>"`.
  2. In `plans/00-INDEX.md` set the task to `✅ Done` and commit on `main`: `[Task NN] Mark task done`.
  3. Keep the task branch (deleting branches needs approval, G-DESTRUCT-2).
  4. If this was Task 18, tell the client the project is complete and stop. Otherwise continue with the next task from **4. Start the task**.
- **Changes requested:** make them on the same branch, commit, and go back to **6. Review loop**.
- **"no" / "stop":** leave the branch as it is, keep the status `🟡 Awaiting merge`, and stop. The client can continue later with `/start-process`.
