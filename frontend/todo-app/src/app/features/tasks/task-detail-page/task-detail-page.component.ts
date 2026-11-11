import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Placeholder page (Task 10). The real page is built in Task 13. */
@Component({
  selector: 'app-task-detail-page',
  template: `<h1>Task {{ id() }}</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskDetailPageComponent {
  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();
}
