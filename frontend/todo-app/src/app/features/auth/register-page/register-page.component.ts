import { ChangeDetectionStrategy, Component } from '@angular/core';

/** Placeholder page (Task 10). The real page is built in a later task. */
@Component({
  selector: 'app-register-page',
  template: `<h1>Register</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPageComponent {}
