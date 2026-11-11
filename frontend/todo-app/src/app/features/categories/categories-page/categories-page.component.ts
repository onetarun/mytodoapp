import { HttpStatusCode } from '@angular/common/http';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  Injector,
  OnInit,
  signal,
  viewChild,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { filter, finalize, switchMap } from 'rxjs';

import { ApiError } from '../../../core/http/api-error';
import { Category } from '../../../core/models/category';
import { ToastService } from '../../../core/ui/toast.service';
import { ConfirmService } from '../../../shared/confirm-dialog/confirm.service';
import { notBlankValidator } from '../../../shared/validators/not-blank.validator';
import { CategoryService } from '../category.service';
import { CATEGORY_NAME_MAX_LENGTH } from '../category-rules';

const NAME_FIELD = 'name';

/** Lists the user's categories with task counts; add, rename inline and delete with confirmation (FR-15, FR-16). */
@Component({
  selector: 'app-categories-page',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    ReactiveFormsModule,
  ],
  templateUrl: './categories-page.component.html',
  styleUrl: './categories-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesPageComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);
  private readonly injector = inject(Injector);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly addInput = viewChild.required<ElementRef<HTMLInputElement>>('addInput');
  private readonly editInput = viewChild<ElementRef<HTMLInputElement>>('editInput');

  protected readonly maxLength = CATEGORY_NAME_MAX_LENGTH;
  protected readonly categories = this.categoryService.categories;
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly editingId = signal<number | null>(null);

  // Each form has a FormGroup so Angular handles submit (Enter) instead of the browser.
  protected readonly addForm = new FormGroup({ name: categoryNameControl() });
  protected readonly editForm = new FormGroup({ name: categoryNameControl() });
  protected readonly newName = this.addForm.controls.name;
  protected readonly editName = this.editForm.controls.name;

  ngOnInit(): void {
    this.categoryService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({ error: () => undefined });
  }

  protected add(): void {
    if (this.newName.invalid) {
      this.newName.markAsTouched();
      this.addInput().nativeElement.focus();
      return;
    }

    const name = this.newName.value.trim();
    this.busy.set(true);
    this.categoryService
      .create({ name })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.newName.reset();
          this.toast.success(`Category "${name}" added.`);
          this.addInput().nativeElement.focus();
        },
        error: (error: unknown) => {
          this.showFieldError(this.newName, error, 'Could not add the category.');
          this.addInput().nativeElement.focus();
        },
      });
  }

  protected startEdit(category: Category): void {
    this.editName.reset(category.name);
    this.editingId.set(category.id);
    afterNextRender(() => this.editInput()?.nativeElement.select(), { injector: this.injector });
  }

  protected cancelEdit(): void {
    const id = this.editingId();
    this.editingId.set(null);
    this.focusRowAction(id, 'rename');
  }

  protected saveEdit(category: Category): void {
    if (this.editName.invalid) {
      this.editName.markAsTouched();
      this.editInput()?.nativeElement.focus();
      return;
    }

    const name = this.editName.value.trim();
    if (name === category.name) {
      this.cancelEdit();
      return;
    }

    this.busy.set(true);
    this.categoryService
      .update(category.id, { name })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.editingId.set(null);
          this.toast.success(`Category renamed to "${name}".`);
          this.focusRowAction(category.id, 'rename');
        },
        error: (error: unknown) => {
          this.showFieldError(this.editName, error, 'Could not rename the category.');
          this.editInput()?.nativeElement.focus();
        },
      });
  }

  protected remove(category: Category): void {
    const taskWord = category.taskCount === 1 ? 'task' : 'tasks';
    this.confirm
      .confirm({
        title: 'Delete category',
        message: `Delete '${category.name}'? Its ${category.taskCount} ${taskWord} will become Uncategorized.`,
        confirmLabel: 'Delete',
      })
      .pipe(
        filter((confirmed) => confirmed),
        switchMap(() => {
          this.busy.set(true);
          return this.categoryService
            .delete(category.id)
            .pipe(finalize(() => this.busy.set(false)));
        }),
      )
      .subscribe({
        next: () => {
          this.toast.success(`Category "${category.name}" deleted.`);
          this.addInput().nativeElement.focus();
        },
        error: (error: unknown) => this.toastUnhandled(error, 'Could not delete the category.'),
      });
  }

  /** Shows the server's name error (e.g. duplicate) under the field; other API errors as a toast. */
  private showFieldError(control: FormControl<string>, error: unknown, fallback: string): void {
    const message = error instanceof ApiError ? error.fieldError(NAME_FIELD) : null;
    if (message !== null) {
      control.setErrors({ server: message });
      control.markAsTouched();
      this.toast.error(message);
      return;
    }

    this.toastUnhandled(error, fallback);
  }

  /** 404 (deleted elsewhere) and other 4xx get a toast; network/5xx/429 were already shown by the interceptor. */
  private toastUnhandled(error: unknown, fallback: string): void {
    const isClientError =
      error instanceof ApiError &&
      error.status >= HttpStatusCode.BadRequest &&
      error.status < HttpStatusCode.InternalServerError &&
      error.status !== HttpStatusCode.TooManyRequests;
    if (isClientError) {
      const notFound = error.status === HttpStatusCode.NotFound;
      this.toast.error(notFound ? 'This category no longer exists.' : fallback);
      if (notFound) {
        this.editingId.set(null);
        this.categoryService.getAll().subscribe({ error: () => undefined });
      }
    }
  }

  private focusRowAction(id: number | null, action: 'rename' | 'delete'): void {
    if (id === null) {
      return;
    }
    afterNextRender(
      () =>
        this.host.nativeElement
          .querySelector<HTMLElement>(`[data-category-id="${id}"][data-action="${action}"]`)
          ?.focus(),
      { injector: this.injector },
    );
  }
}

function categoryNameControl(): FormControl<string> {
  return new FormControl('', {
    nonNullable: true,
    validators: [notBlankValidator, Validators.maxLength(CATEGORY_NAME_MAX_LENGTH)],
  });
}
