/** Moves keyboard focus to the first invalid field inside `form` (NFR-23). */
export function focusFirstInvalid(form: HTMLElement): void {
  const firstInvalid = form.querySelector<HTMLElement>(
    'input.ng-invalid, textarea.ng-invalid, select.ng-invalid',
  );
  firstInvalid?.focus();
}
