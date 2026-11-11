import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../auth/auth.service';

/** Logged-in layout: header with navigation, bell, user name and logout, then the page content. */
@Component({
  selector: 'app-shell',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatToolbarModule,
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

  protected readonly user = this.auth.currentUser;

  protected readonly navLinks = [
    { path: '/dashboard', label: 'Dashboard' },
    { path: '/tasks', label: 'Tasks' },
    { path: '/categories', label: 'Categories' },
  ] as const;

  protected logout(): void {
    this.auth.logout();
  }
}
