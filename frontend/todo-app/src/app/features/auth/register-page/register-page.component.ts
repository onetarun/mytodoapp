import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { ApiError } from '../../../core/http/api-error';
import { applyServerErrors } from '../../../shared/forms/apply-server-errors';
import { focusFirstInvalid } from '../../../shared/forms/focus-first-invalid';
import { matchesControlValidator } from '../../../shared/validators/matches-control.validator';
import { passwordRulesValidator } from '../../../shared/validators/password-rules.validator';
import { AUTH_FORM_RULES } from '../auth-form-rules';

/** Register page (FR-1): name, email, password and confirmation; logs the user in on success. */
@Component({
  selector: 'app-register-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './register-page.component.html',
  styleUrl: '../auth-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly formElement = viewChild.required<ElementRef<HTMLFormElement>>('formElement');

  protected readonly rules = AUTH_FORM_RULES;
  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    name: ['', [Validators.required, Validators.maxLength(AUTH_FORM_RULES.nameMaxLength)]],
    email: [
      '',
      [Validators.required, Validators.email, Validators.maxLength(AUTH_FORM_RULES.emailMaxLength)],
    ],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(AUTH_FORM_RULES.passwordMinLength),
        passwordRulesValidator,
      ],
    ],
    confirmPassword: ['', [Validators.required, matchesControlValidator('password')]],
  });

  constructor() {
    // Re-check the confirmation whenever the password changes.
    this.form.controls.password.valueChanges
      .pipe(takeUntilDestroyed(inject(DestroyRef)))
      .subscribe(() => this.form.controls.confirmPassword.updateValueAndValidity());
  }

  protected submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalid(this.formElement().nativeElement, this.form);
      return;
    }

    const { name, email, password } = this.form.getRawValue();
    this.submitting.set(true);
    this.auth
      .register({ name: name.trim(), email: email.trim(), password })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => void this.router.navigate(['/dashboard']),
        error: (error: unknown) => this.showError(error),
      });
  }

  private showError(error: unknown): void {
    if (!(error instanceof ApiError) || error.status !== 400) {
      return; // Network and server errors are shown by the error interceptor.
    }

    // Field errors go under their fields; anything else (or a 400 without fields) shows above the form.
    const unmatched = applyServerErrors(this.form, error);
    const shownOnFields = Object.keys(error.fieldErrors).length > unmatched.length;
    this.formError.set(unmatched[0] ?? (shownOnFields ? null : error.message));
    focusFirstInvalid(this.formElement().nativeElement, this.form);
  }
}
