import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject } from '@angular/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';

import { NotificationService } from '../notification.service';

/**
 * Header bell (FR-19, FR-20): a badge with the count (hidden at 0) and a menu with an
 * "Overdue" group and a "Due within 24 hours" group linking to each task. The menu closes
 * with Esc or a click outside. Polling runs while the bell is on screen (logged-in layout only).
 */
@Component({
  selector: 'app-notification-bell',
  imports: [
    DatePipe,
    MatBadgeModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    RouterLink,
  ],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationBellComponent {
  private readonly notificationService = inject(NotificationService);

  protected readonly count = this.notificationService.count;
  protected readonly overdue = this.notificationService.overdue;
  protected readonly dueSoon = this.notificationService.dueSoon;
  protected readonly ariaLabel = computed(() => {
    const count = this.count();
    return `Notifications, ${count} ${count === 1 ? 'item' : 'items'}`;
  });

  constructor() {
    this.notificationService.start();
    inject(DestroyRef).onDestroy(() => this.notificationService.stop());
  }
}
