import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { NotificationBellComponent } from '../../../features/notifications/notification-bell/notification-bell.component';
import { NotificationService } from '../../../features/notifications/notification.service';
import { AuthService } from '../../auth/auth.service';

/**
 * Logged-in layout: header with navigation, bell, user name and logout, then the page content.
 * Below 768 px the links, user name and logout move into a "Main menu"; the bell stays visible (NFR-20).
 */
@Component({
  selector: 'app-shell',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatToolbarModule,
    NotificationBellComponent,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
  ],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  private readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);

  protected readonly user = this.auth.currentUser;

  protected readonly navLinks = [
    { path: '/dashboard', label: 'Dashboard' },
    { path: '/tasks', label: 'Tasks' },
    { path: '/categories', label: 'Categories' },
  ] as const;

  /** Stops the bell polling before the token is cleared, so no request goes out without it. */
  protected logout(): void {
    this.notifications.stop();
    this.auth.logout();
  }
}
