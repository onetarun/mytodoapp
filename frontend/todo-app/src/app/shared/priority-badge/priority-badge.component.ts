import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { Priority } from '../../core/models/priority';

/** Priority shown as text in a colored pill, so it never relies on color alone (NFR-24). */
@Component({
  selector: 'app-priority-badge',
  template: `<span class="badge" [class]="'badge priority-' + priority().toLowerCase()"
    >{{ priority() }}<span class="visually-hidden"> priority</span></span
  >`,
  styles: `
    .badge {
      display: inline-block;
      padding: 2px var(--app-space-2);
      border-radius: 12px;
      font: var(--mat-sys-label-medium);
      border: 1px solid currentColor;
      white-space: nowrap;
    }
    .priority-high {
      background: #fde7e7;
      color: #8c1d18;
    }
    .priority-medium {
      background: #fff3d6;
      color: #6b4a00;
    }
    .priority-low {
      background: #e8eef4;
      color: #36474f;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriorityBadgeComponent {
  readonly priority = input.required<Priority>();
}
