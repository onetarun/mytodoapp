import { DatePipe, TitleCasePipe } from '@angular/common';
import { HttpStatusCode } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleChange, MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink } from '@angular/router';
import { filter, finalize, switchMap } from 'rxjs';

import { ApiError } from '../../../core/http/api-error';
import { TaskListItem } from '../../../core/models/task-list-item';
import { TaskStatusFilter } from '../../../core/models/task-status-filter';
import { TaskWriteRequest } from '../../../core/models/task-write-request';
import { ToastService } from '../../../core/ui/toast.service';
import { ConfirmService } from '../../../shared/confirm-dialog/confirm.service';
import { PriorityBadgeComponent } from '../../../shared/priority-badge/priority-badge.component';
import { TaskEditDialogService } from '../task-edit-dialog/task-edit-dialog.service';
import { TaskFormComponent } from '../task-form/task-form.component';
import { parseStatusFilter, TASK_RULES } from '../task-rules';
import { TaskService } from '../task.service';

const EMPTY_MESSAGES: Record<TaskStatusFilter, string> = {
  all: 'No tasks yet — add your first one',
  pending: 'No pending tasks 🎉',
  completed: 'No completed tasks yet',
};

/**
 * Main task screen (FR-6 to FR-10a, FR-21): quick add, status filter kept in `?status=`,
 * task rows with complete checkbox, overdue marker, edit and delete.
 */
@Component({
  selector: 'app-task-list-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatButtonToggleModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    PriorityBadgeComponent,
    RouterLink,
    TaskFormComponent,
    TitleCasePipe,
  ],
  templateUrl: './task-list-page.component.html',
  styleUrl: './task-list-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskListPageComponent {
  private readonly taskService = inject(TaskService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);
  private readonly editDialog = inject(TaskEditDialogService);

  /** Bound from the `?status=` query parameter. */
  readonly status = input<string | undefined>();

  protected readonly statusFilters = TASK_RULES.statusFilters;
  protected readonly activeStatus = computed(() => parseStatusFilter(this.status()));
  protected readonly emptyMessage = computed(() => EMPTY_MESSAGES[this.activeStatus()]);
  protected readonly tasks = signal<TaskListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly detailsOpen = signal(false);
  protected readonly busyTaskId = signal<number | null>(null);

  protected readonly createTask = (request: TaskWriteRequest) => this.taskService.create(request);

  constructor() {
    // Reload whenever the filter in the URL changes (also on first load and refresh).
    effect(() => {
      const status = this.activeStatus();
      untracked(() => this.load(status));
    });
  }

  protected changeStatus(change: MatButtonToggleChange): void {
    const status = change.value as TaskStatusFilter;
    void this.router.navigate([], {
      queryParams: { status: status === 'all' ? null : status },
      queryParamsHandling: 'merge',
    });
  }

  protected onCreated(): void {
    this.toast.success('Task added.');
    this.load(this.activeStatus());
  }

  protected toggleComplete(task: TaskListItem): void {
    this.busyTaskId.set(task.id);
    this.taskService
      .setCompletion(task.id, !task.isCompleted)
      .pipe(finalize(() => this.busyTaskId.set(null)))
      .subscribe({
        next: () => {
          this.toast.success(task.isCompleted ? 'Task marked as pending.' : 'Task completed.');
          this.load(this.activeStatus());
        },
        error: (error: unknown) => this.handleRowError(error, 'Could not update the task.'),
      });
  }

  protected edit(task: TaskListItem): void {
    this.busyTaskId.set(task.id);
    this.taskService
      .getById(task.id)
      .pipe(
        finalize(() => this.busyTaskId.set(null)),
        switchMap((detail) => this.editDialog.open(detail)),
        filter((saved) => saved),
      )
      .subscribe({
        next: () => {
          this.toast.success('Task updated.');
          this.load(this.activeStatus());
        },
        error: (error: unknown) => this.handleRowError(error, 'Could not open the task.'),
      });
  }

  protected remove(task: TaskListItem): void {
    this.confirm
      .confirm({
        title: 'Delete task',
        message: `Delete '${task.title}'? This cannot be undone from the app.`,
        confirmLabel: 'Delete',
      })
      .pipe(
        filter((confirmed) => confirmed),
        switchMap(() => {
          this.busyTaskId.set(task.id);
          return this.taskService.delete(task.id).pipe(finalize(() => this.busyTaskId.set(null)));
        }),
      )
      .subscribe({
        next: () => {
          this.toast.success('Task deleted.');
          this.load(this.activeStatus());
        },
        error: (error: unknown) => this.handleRowError(error, 'Could not delete the task.'),
      });
  }

  protected progressLabel(task: TaskListItem): string {
    return `${task.subTaskCompleted}/${task.subTaskTotal}`;
  }

  private load(status: TaskStatusFilter): void {
    this.loading.set(true);
    this.taskService
      .getAll(status)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (tasks) => this.tasks.set(tasks),
        error: () => this.tasks.set([]),
      });
  }

  /** 404 (deleted elsewhere) and other 4xx get a toast; network/5xx/429 were shown by the interceptor. */
  private handleRowError(error: unknown, fallback: string): void {
    if (!(error instanceof ApiError)) {
      return;
    }

    if (error.status === HttpStatusCode.NotFound) {
      this.toast.error('This task no longer exists.');
      this.load(this.activeStatus());
    } else if (
      error.status >= HttpStatusCode.BadRequest &&
      error.status < HttpStatusCode.InternalServerError &&
      error.status !== HttpStatusCode.TooManyRequests
    ) {
      this.toast.error(fallback);
    }
  }
}
