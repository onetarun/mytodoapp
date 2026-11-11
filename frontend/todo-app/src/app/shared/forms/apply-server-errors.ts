import { FormGroup } from '@angular/forms';

import { ApiError } from '../../core/http/api-error';

/**
 * Puts ProblemDetails field errors (camelCase keys) on the matching form controls as `{ server: message }`.
 * Returns the messages that have no matching control, to show at form level.
 */
export function applyServerErrors(form: FormGroup, error: ApiError): string[] {
  const unmatched: string[] = [];

  for (const [field, messages] of Object.entries(error.fieldErrors)) {
    const control = form.get(field);
    const message = messages[0];
    if (message === undefined) {
      continue;
    }

    if (control) {
      control.setErrors({ ...control.errors, server: message });
      control.markAsTouched();
    } else {
      unmatched.push(message);
    }
  }

  return unmatched;
}
