import { inject, Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { map, Observable } from 'rxjs';

import { TaskDetail } from '../../../core/models/task-detail';
import { TaskEditDialogComponent } from './task-edit-dialog.component';

/** Opens the edit dialog; emits `true` when the task was saved. */
@Injectable({ providedIn: 'root' })
export class TaskEditDialogService {
  private readonly dialog = inject(MatDialog);

  open(task: TaskDetail): Observable<boolean> {
    return this.dialog
      .open<TaskEditDialogComponent, TaskDetail, boolean>(TaskEditDialogComponent, {
        data: task,
        width: '640px',
        maxWidth: '95vw',
        autoFocus: 'first-tabbable',
        restoreFocus: true,
      })
      .afterClosed()
      .pipe(map((saved) => saved === true));
  }
}
