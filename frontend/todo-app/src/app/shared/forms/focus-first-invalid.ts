import { FormGroup } from '@angular/forms';

/**
 * Moves keyboard focus to the first invalid field of `form` (NFR-23).
 * Uses the control state, not CSS classes, so it also works right after server errors are set.
 * Controls are checked in declaration order, which matches the field order on screen.
 */
export function focusFirstInvalid(formElement: HTMLElement, form: FormGroup): void {
  const firstInvalid = Object.keys(form.controls).find((name) => form.controls[name]?.invalid);
  if (firstInvalid === undefined) {
    return;
  }

  formElement.querySelector<HTMLElement>(`[formcontrolname="${firstInvalid}"]`)?.focus();
}
