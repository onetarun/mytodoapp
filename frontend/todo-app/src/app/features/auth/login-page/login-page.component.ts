import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { HttpStatusCode } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { ApiError } from '../../../core/http/api-error';
import { focusFirstInvalid } from '../../../shared/forms/focus-first-invalid';

const DEFAULT_REDIRECT = '/dashboard';

/** Messages for the login error statuses (FR-2, NFR-11, NFR-15). */
export const LOGIN_MESSAGES = {
  invalidCredentials: 'Invalid email or password',
  lockedOut: 'Account locked after too many attempts. Try again in 15 minutes.',
  tooManyRequests: 'Too many attempts, please wait a minute.',
} as const;

/** Login page (FR-2). Goes to `returnUrl` (from the auth guard) or the dashboard on success. */
@Component({
  selector: 'app-login-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './login-page.component.html',
  styleUrl: '../auth-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly formElement = viewChild.required<ElementRef<HTMLFormElement>>('formElement');

  /** Bound from the `?returnUrl=` query parameter set by the auth guard. */
  readonly returnUrl = input<string | undefined>();

  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  protected submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalid(this.formElement().nativeElement);
      return;
    }

    const { email, password } = this.form.getRawValue();
    this.submitting.set(true);
    this.auth
      .login({ email: email.trim(), password })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => void this.router.navigateByUrl(safeReturnUrl(this.returnUrl())),
        error: (error: unknown) => this.formError.set(loginErrorMessage(error)),
      });
  }
}

/** Only same-app paths are allowed, so a crafted link cannot redirect to another site. */
function safeReturnUrl(returnUrl: string | undefined): string {
  const isInternalPath =
    returnUrl !== undefined &&
    returnUrl.startsWith('/') &&
    !returnUrl.startsWith('//') &&
    !returnUrl.startsWith('/\\') &&
    !returnUrl.startsWith('/login');
  return isInternalPath ? returnUrl : DEFAULT_REDIRECT;
}

function loginErrorMessage(error: unknown): string | null {
  if (!(error instanceof ApiError)) {
    return null;
  }

  switch (error.status) {
    case HttpStatusCode.Unauthorized:
      return LOGIN_MESSAGES.invalidCredentials;
    case HttpStatusCode.Locked:
      return LOGIN_MESSAGES.lockedOut;
    case HttpStatusCode.TooManyRequests:
      return LOGIN_MESSAGES.tooManyRequests;
    case HttpStatusCode.BadRequest:
      return error.fieldErrors['email']?.[0] ?? error.fieldErrors['password']?.[0] ?? error.message;
    default:
      return null; // Network and server errors are shown by the error interceptor.
  }
}
