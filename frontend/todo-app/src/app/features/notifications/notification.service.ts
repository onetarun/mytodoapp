import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { catchError, EMPTY, filter, Observable, Subscription, switchMap, timer } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsResponse } from '../../core/models/notifications-response';
import { TaskNotification } from '../../core/models/task-notification';

const NOTIFICATIONS_URL = `${environment.apiBaseUrl}/notifications`;

/** How often the bell refreshes while the app is open (FR-22: every 5 minutes). */
export const NOTIFICATION_POLL_INTERVAL_MS = 5 * 60 * 1000;

/**
 * Overdue and due-within-24-hours tasks for the header bell (FR-19, FR-20, FR-22).
 * `start()` fetches now and then every 5 minutes; `stop()` ends polling (on logout).
 * `refresh()` re-fetches once, e.g. after a task was created, edited, completed or deleted.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly state = signal<NotificationsResponse>({ count: 0, items: [] });
  private polling: Subscription | undefined;
  private pending: Subscription | undefined;

  readonly notifications = this.state.asReadonly();
  readonly count = computed(() => this.state().count);
  readonly overdue = computed(() => this.byType('Overdue'));
  readonly dueSoon = computed(() => this.byType('DueSoon'));

  start(): void {
    this.stop();
    this.polling = timer(0, NOTIFICATION_POLL_INTERVAL_MS)
      .pipe(
        // Never call the API without a valid session, so a logout never causes a 401.
        filter(() => this.auth.isLoggedIn()),
        switchMap(() => this.fetch()),
      )
      .subscribe((response) => this.state.set(response));
  }

  stop(): void {
    this.polling?.unsubscribe();
    this.polling = undefined;
    this.pending?.unsubscribe();
    this.pending = undefined;
    this.state.set({ count: 0, items: [] });
  }

  refresh(): void {
    if (this.polling === undefined || !this.auth.isLoggedIn()) {
      return;
    }

    this.pending?.unsubscribe();
    this.pending = this.fetch().subscribe((response) => this.state.set(response));
  }

  /** A failed fetch keeps the last known list; the error interceptor already told the user. */
  private fetch(): Observable<NotificationsResponse> {
    return this.http.get<NotificationsResponse>(NOTIFICATIONS_URL).pipe(catchError(() => EMPTY));
  }

  private byType(type: TaskNotification['type']): TaskNotification[] {
    return this.state().items.filter((item) => item.type === type);
  }
}
