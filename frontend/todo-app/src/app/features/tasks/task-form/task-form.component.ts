import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  input,
  OnInit,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { HttpStatusCode } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { finalize, Observable } from 'rxjs';

import { ApiError } from '../../../core/http/api-error';
import { Priority } from '../../../core/models/priority';
import { TaskDetail } from '../../../core/models/task-detail';
import { TaskWriteRequest } from '../../../core/models/task-write-request';
import { ToastService } from '../../../core/ui/toast.service';
import { fromLocalDateTimeInput, toLocalDateTimeInput } from '../../../shared/dates/date-time';
import { applyServerErrors } from '../../../shared/forms/apply-server-errors';
import { focusFirstInvalid } from '../../../shared/forms/focus-first-invalid';
import { notBlankValidator } from '../../../shared/validators/not-blank.validator';
import { CategoryService } from '../../categories/category.service';
import { TASK_RULES } from '../task-rules';

/**
 * Task create/edit form (FR-6, FR-7). The title is always shown; description, due date, priority
 * and category appear when `showDetails` is true. The parent passes `save`, which does the API call;
 * this component shows validation and server errors and emits `saved` on success.
 */
@Component({
  selector: 'app-task-form',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule,
  ],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskFormComponent implements OnInit {
  private readonly toast = inject(ToastService);
  private readonly categoryService = inject(CategoryService);
  private readonly formElement = viewChild.required<ElementRef<HTMLFormElement>>('formElement');
  private readonly titleInput = viewChild.required<ElementRef<HTMLInputElement>>('titleInput');

  /** Task to edit; `null` creates a new task. */
  readonly task = input<TaskDetail | null>(null);
  readonly showDetails = input(true);
  readonly submitLabel = input('Save');
  readonly cancellable = input(false);
  /** Clears the form after a successful save (quick add). */
  readonly resetAfterSave = input(false);
  readonly save = input.required<(request: TaskWriteRequest) => Observable<unknown>>();

  readonly saved = output();
  readonly cancelled = output();

  protected readonly rules = TASK_RULES;
  protected readonly categories = this.categoryService.categories;
  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = new FormGroup({
    title: new FormControl('', {
      nonNullable: true,
      validators: [notBlankValidator, Validators.maxLength(TASK_RULES.titleMaxLength)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(TASK_RULES.descriptionMaxLength)],
    }),
    dueDate: new FormControl('', { nonNullable: true }),
    priority: new FormControl<Priority>(TASK_RULES.defaultPriority, { nonNullable: true }),
    categoryId: new FormControl<number | null>(null),
  });

  ngOnInit(): void {
    this.categoryService.ensureLoaded();
    const task = this.task();
    if (task !== null) {
      this.form.setValue({
        title: task.title,
        description: task.description ?? '',
        dueDate: toLocalDateTimeInput(task.dueDate),
        priority: task.priority,
        categoryId: task.categoryId,
      });
    }
  }

  /** Puts keyboard focus on the title field. */
  focusTitle(): void {
    this.titleInput().nativeElement.focus();
  }

  protected submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalid(this.formElement().nativeElement, this.form);
      return;
    }

    this.submitting.set(true);
    this.save()(this.toRequest())
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          if (this.resetAfterSave()) {
            this.form.reset();
            this.focusTitle();
          }
          this.saved.emit();
        },
        error: (error: unknown) => this.showError(error),
      });
  }

  private toRequest(): TaskWriteRequest {
    const value = this.form.getRawValue();
    const description = value.description.trim();
    return {
      title: value.title.trim(),
      description: description === '' ? null : description,
      dueDate: fromLocalDateTimeInput(value.dueDate),
      priority: value.priority,
      categoryId: value.categoryId,
    };
  }

  private showError(error: unknown): void {
    if (!(error instanceof ApiError)) {
      return;
    }

    if (error.status === HttpStatusCode.BadRequest) {
      const unmatched = applyServerErrors(this.form, error);
      const shownOnFields = Object.keys(error.fieldErrors).length > unmatched.length;
      this.formError.set(unmatched[0] ?? (shownOnFields ? null : error.message));
      focusFirstInvalid(this.formElement().nativeElement, this.form);
    } else if (error.status === HttpStatusCode.NotFound) {
      this.toast.error('This task no longer exists.');
    }
    // Network, 5xx and 429 errors are shown by the error interceptor.
  }
}
