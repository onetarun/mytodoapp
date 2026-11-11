import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/auth-response';
import { LoginRequest } from '../models/login-request';
import { RegisterRequest } from '../models/register-request';
import { ToastService } from '../ui/toast.service';

/** sessionStorage key; the session is cleared when the tab closes (ARCHITECTURE §7). */
const SESSION_STORAGE_KEY = 'todo-app.session';
const SESSION_EXPIRED_MESSAGE = 'Your session has expired. Please log in again.';

/**
 * Login, register and logout. Keeps `{ token, expiresAt, user }` in sessionStorage
 * and logs the user out automatically when the token expires (NFR-10).
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly session = signal<AuthResponse | null>(readStoredSession());
  private expiryTimer: ReturnType<typeof setTimeout> | undefined;

  readonly token = computed(() => this.session()?.token ?? null);
  readonly currentUser = computed(() => this.session()?.user ?? null);

  constructor() {
    if (this.isLoggedIn()) {
      this.scheduleAutoLogout();
    } else {
      this.clearSession();
    }
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, request)
      .pipe(tap((response) => this.startSession(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/auth/register`, request)
      .pipe(tap((response) => this.startSession(response)));
  }

  /** Clears the token on the client and goes to the login page (FR-5). */
  logout(): void {
    this.clearSession();
    void this.router.navigate(['/login']);
  }

  /** True when a token exists and has not expired yet. */
  isLoggedIn(): boolean {
    const current = this.session();
    return current !== null && Date.parse(current.expiresAt) > Date.now();
  }

  private startSession(response: AuthResponse): void {
    sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(response));
    this.session.set(response);
    this.scheduleAutoLogout();
  }

  private clearSession(): void {
    clearTimeout(this.expiryTimer);
    this.expiryTimer = undefined;
    sessionStorage.removeItem(SESSION_STORAGE_KEY);
    this.session.set(null);
  }

  private scheduleAutoLogout(): void {
    clearTimeout(this.expiryTimer);
    const current = this.session();
    if (current === null) {
      return;
    }

    const remainingMs = Math.max(Date.parse(current.expiresAt) - Date.now(), 0);
    this.expiryTimer = setTimeout(() => {
      this.toast.error(SESSION_EXPIRED_MESSAGE);
      this.logout();
    }, remainingMs);
  }
}

function readStoredSession(): AuthResponse | null {
  try {
    const raw = sessionStorage.getItem(SESSION_STORAGE_KEY);
    if (raw === null) {
      return null;
    }

    const parsed = JSON.parse(raw) as Partial<AuthResponse>;
    const isValid =
      typeof parsed.token === 'string' &&
      typeof parsed.expiresAt === 'string' &&
      typeof parsed.user?.id === 'string';
    return isValid ? (parsed as AuthResponse) : null;
  } catch {
    return null;
  }
}
