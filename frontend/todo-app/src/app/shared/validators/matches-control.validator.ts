import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * The control's value must equal the sibling control `otherControlName` (e.g. confirm password).
 * Re-validate this control when the other one changes.
 */
export function matchesControlValidator(otherControlName: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const other = control.parent?.get(otherControlName);
    if (!other || control.value === '' || control.value === other.value) {
      return null;
    }

    return { mismatch: true };
  };
}
