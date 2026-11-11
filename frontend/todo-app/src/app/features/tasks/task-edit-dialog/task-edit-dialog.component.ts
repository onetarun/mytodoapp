import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import { TaskDetail } from '../../../core/models/task-detail';
import { TaskWriteRequest } from '../../../core/models/task-write-request';
import { TaskFormComponent } from '../task-form/task-form.component';
import { TaskService } from '../task.service';

/** Dialog that edits every field of a task (FR-7). Closes with `true` after a successful save. */
@Component({
  selector: 'app-task-edit-dialog',
  imports: [MatDialogModule, TaskFormComponent],
  template: `
    <h2 mat-dialog-title>Edit task</h2>
    <mat-dialog-content>
      <app-task-form
        [task]="task"
        [save]="save"
        submitLabel="Save"
        [cancellable]="true"
        (saved)="dialogRef.close(true)"
        (cancelled)="dialogRef.close(false)"
      />
    </mat-dialog-content>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskEditDialogComponent {
  private readonly taskService = inject(TaskService);

  protected readonly dialogRef =
    inject<MatDialogRef<TaskEditDialogComponent, boolean>>(MatDialogRef);
  protected readonly task = inject<TaskDetail>(MAT_DIALOG_DATA);
  protected readonly save = (request: TaskWriteRequest) =>
    this.taskService.update(this.task.id, request);
}
