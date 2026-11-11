import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

const SUCCESS_DURATION_MS = 3000;
const ERROR_DURATION_MS = 6000;
const DISMISS_LABEL = 'Close';

/** Short success and error messages shown in a snackbar (screen readers announce them). */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, DISMISS_LABEL, {
      duration: SUCCESS_DURATION_MS,
      panelClass: 'toast-success',
      politeness: 'polite',
    });
  }

  error(message: string): void {
    this.snackBar.open(message, DISMISS_LABEL, {
      duration: ERROR_DURATION_MS,
      panelClass: 'toast-error',
      politeness: 'assertive',
    });
  }
}
