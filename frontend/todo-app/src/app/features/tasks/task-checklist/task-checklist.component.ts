import { HttpStatusCode } from '@angular/common/http';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  inject,
  Injector,
  input,
  linkedSignal,
  signal,
  viewChild,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs';

import { ApiError } from '../../../core/http/api-error';
import { SubTask } from '../../../core/models/sub-task';
import { ToastService } from '../../../core/ui/toast.service';
import { notBlankValidator } from '../../../shared/validators/not-blank.validator';
import { SubTaskService } from '../sub-task.service';
import { TASK_RULES } from '../task-rules';

const TITLE_FIELD = 'title';
const PERCENT = 100;

/**
 * Checklist of one task (FR-11 to FR-13): progress "x of y done" with a bar, tick/untick,
 * inline rename, delete (toast, no dialog) and an "Add item" field that keeps focus for fast entry.
 * The list page reads fresh progress from the API, so it is up to date when the user goes back.
 */
@Component({
  selector: 'app-task-checklist',
  imports: [
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatTooltipModule,
    ReactiveFormsModule,
  ],
  templateUrl: './task-checklist.component.html',
  styleUrl: './task-checklist.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskChecklistComponent {
  private readonly subTaskService = inject(SubTaskService);
  private readonly toast = inject(ToastService);
  private readonly injector = inject(Injector);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly addInput = viewChild.required<ElementRef<HTMLInputElement>>('addInput');
  private readonly editInput = viewChild<ElementRef<HTMLInputElement>>('editInput');

  readonly taskId = input.required<number>();
  readonly subTasks = input.required<SubTask[]>();

  /** Local copy, kept in `sortOrder`; starts from the task detail and follows every API response. */
  protected readonly items = linkedSignal(() => sortBySortOrder(this.subTasks()));
  protected readonly doneCount = computed(
    () => this.items().filter((item) => item.isCompleted).length,
  );
  protected readonly progressPercent = computed(() => {
    const total = this.items().length;
    return total === 0 ? 0 : Math.round((this.doneCount() / total) * PERCENT);
  });

  protected readonly maxLength = TASK_RULES.titleMaxLength;
  protected readonly busyId = signal<number | null>(null);
  protected readonly adding = signal(false);
  protected readonly editingId = signal<number | null>(null);

  protected readonly addForm = new FormGroup({ title: subTaskTitleControl() });
  protected readonly editForm = new FormGroup({ title: subTaskTitleControl() });

  protected add(): void {
    const control = this.addForm.controls.title;
    if (control.invalid) {
      control.markAsTouched();
      this.addInput().nativeElement.focus();
      return;
    }

    this.adding.set(true);
    this.subTaskService
      .create(this.taskId(), { title: control.value.trim() })
      .pipe(finalize(() => this.adding.set(false)))
      .subscribe({
        next: (created) => {
          this.items.update((items) => sortBySortOrder([...items, created]));
          this.addForm.reset();
          this.addInput().nativeElement.focus();
        },
        error: (error: unknown) => {
          this.showFieldError(control, error);
          this.addInput().nativeElement.focus();
        },
      });
  }

  protected toggle(item: SubTask): void {
    this.busyId.set(item.id);
    this.subTaskService
      .setCompletion(this.taskId(), item.id, !item.isCompleted)
      .pipe(finalize(() => this.busyId.set(null)))
      .subscribe({
        next: (updated) => this.replace(updated),
        error: (error: unknown) => this.handleError(error),
      });
  }

  protected startEdit(item: SubTask): void {
    this.editForm.reset({ title: item.title });
    this.editingId.set(item.id);
    afterNextRender(() => this.editInput()?.nativeElement.select(), { injector: this.injector });
  }

  protected cancelEdit(): void {
    const id = this.editingId();
    this.editingId.set(null);
    this.focusItemAction(id, 'edit');
  }

  protected saveEdit(item: SubTask): void {
    const control = this.editForm.controls.title;
    if (control.invalid) {
      control.markAsTouched();
      this.editInput()?.nativeElement.focus();
      return;
    }

    const title = control.value.trim();
    if (title === item.title) {
      this.cancelEdit();
      return;
    }

    this.busyId.set(item.id);
    this.subTaskService
      .update(this.taskId(), item.id, { title })
      .pipe(finalize(() => this.busyId.set(null)))
      .subscribe({
        next: (updated) => {
          this.replace(updated);
          this.editingId.set(null);
          this.toast.success('Item updated.');
          this.focusItemAction(item.id, 'edit');
        },
        error: (error: unknown) => {
          this.showFieldError(control, error);
          this.editInput()?.nativeElement.focus();
        },
      });
  }

  protected remove(item: SubTask): void {
    this.busyId.set(item.id);
    this.subTaskService
      .delete(this.taskId(), item.id)
      .pipe(finalize(() => this.busyId.set(null)))
      .subscribe({
        next: () => {
          this.items.update((items) => items.filter((existing) => existing.id !== item.id));
          this.toast.success('Item deleted');
          this.addInput().nativeElement.focus();
        },
        error: (error: unknown) => this.handleError(error),
      });
  }

  private replace(updated: SubTask): void {
    this.items.update((items) => items.map((item) => (item.id === updated.id ? updated : item)));
  }

  private showFieldError(control: FormControl<string>, error: unknown): void {
    const message = error instanceof ApiError ? error.fieldError(TITLE_FIELD) : null;
    if (message !== null) {
      control.setErrors({ server: message });
      control.markAsTouched();
      return;
    }

    this.handleError(error);
  }

  /** 404: the task or item is gone. Network/5xx/429 are shown by the error interceptor. */
  private handleError(error: unknown): void {
    if (error instanceof ApiError && error.status === HttpStatusCode.NotFound) {
      this.toast.error('This item or its task no longer exists. Reload the page.');
    } else if (
      error instanceof ApiError &&
      error.status >= HttpStatusCode.BadRequest &&
      error.status < HttpStatusCode.InternalServerError &&
      error.status !== HttpStatusCode.TooManyRequests
    ) {
      this.toast.error('Could not update the checklist.');
    }
  }

  private focusItemAction(id: number | null, action: 'edit'): void {
    if (id === null) {
      return;
    }
    afterNextRender(
      () =>
        this.host.nativeElement
          .querySelector<HTMLElement>(`[data-item-id="${id}"][data-action="${action}"]`)
          ?.focus(),
      { injector: this.injector },
    );
  }
}

function subTaskTitleControl(): FormControl<string> {
  return new FormControl('', {
    nonNullable: true,
    validators: [notBlankValidator, Validators.maxLength(TASK_RULES.titleMaxLength)],
  });
}

function sortBySortOrder(items: SubTask[]): SubTask[] {
  return [...items].sort((a, b) => a.sortOrder - b.sortOrder);
}
