import { DatePipe } from '@angular/common';
import { HttpStatusCode } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { filter, finalize } from 'rxjs';

import { ApiError } from '../../../core/http/api-error';
import { TaskDetail } from '../../../core/models/task-detail';
import { ToastService } from '../../../core/ui/toast.service';
import { PriorityBadgeComponent } from '../../../shared/priority-badge/priority-badge.component';
import { TaskChecklistComponent } from '../task-checklist/task-checklist.component';
import { TaskEditDialogService } from '../task-edit-dialog/task-edit-dialog.service';
import { TaskService } from '../task.service';

/** Shows every field of one task, an Edit button and the checklist; `404` shows "Task not found" (FR-7, FR-10, FR-11). */
@Component({
  selector: 'app-task-detail-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PriorityBadgeComponent,
    RouterLink,
    TaskChecklistComponent,
  ],
  templateUrl: './task-detail-page.component.html',
  styleUrl: './task-detail-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskDetailPageComponent {
  private readonly taskService = inject(TaskService);
  private readonly editDialog = inject(TaskEditDialogService);
  private readonly toast = inject(ToastService);

  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  protected readonly task = signal<TaskDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);

  constructor() {
    effect(() => {
      const id = Number(this.id());
      untracked(() => this.load(id));
    });
  }

  protected edit(task: TaskDetail): void {
    this.editDialog
      .open(task)
      .pipe(filter((saved) => saved))
      .subscribe(() => {
        this.toast.success('Task updated.');
        this.load(task.id);
      });
  }

  private load(id: number): void {
    this.loading.set(true);
    this.notFound.set(false);
    if (!Number.isInteger(id) || id <= 0) {
      this.task.set(null);
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }

    this.taskService
      .getById(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (task) => this.task.set(task),
        error: (error: unknown) => {
          this.task.set(null);
          this.notFound.set(error instanceof ApiError && error.status === HttpStatusCode.NotFound);
        },
      });
  }
}
