import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Like `Validators.required`, but a value of only spaces also fails (the API trims names). */
export const notBlankValidator: ValidatorFn = (
  control: AbstractControl<string | null>,
): ValidationErrors | null => ((control.value ?? '').trim() === '' ? { required: true } : null);
