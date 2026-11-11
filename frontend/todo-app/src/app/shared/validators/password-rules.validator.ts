import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const LETTER = /\p{L}/u;
const DIGIT = /[0-9]/;

/**
 * Password must contain at least one letter and one digit (NFR-8), the same rule as the API.
 * Length is checked separately with `Validators.minLength`.
 */
export const passwordRulesValidator: ValidatorFn = (
  control: AbstractControl<string | null>,
): ValidationErrors | null => {
  const value = control.value ?? '';
  if (value === '') {
    return null;
  }

  const errors: ValidationErrors = {};
  if (!LETTER.test(value)) {
    errors['passwordLetter'] = true;
  }
  if (!DIGIT.test(value)) {
    errors['passwordDigit'] = true;
  }

  return Object.keys(errors).length > 0 ? errors : null;
};
