import { HttpClient } from '@angular/common/http';
import { effect, inject, Injectable, signal } from '@angular/core';
import { Observable, switchMap, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';
import { Category } from '../../core/models/category';
import { CategoryRequest } from '../../core/models/category-request';

const CATEGORIES_URL = `${environment.apiBaseUrl}/categories`;

/**
 * The current user's categories (FR-15). Keeps a cached `categories` signal for the category page,
 * the task form and the task list; every change re-reads the list so task counts stay correct.
 */
@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly cache = signal<Category[]>([]);
  private readonly loaded = signal(false);

  /** Cached list, ordered by name by the API. */
  readonly categories = this.cache.asReadonly();

  /** True after the first successful load for the current user. */
  readonly isLoaded = this.loaded.asReadonly();

  constructor() {
    // A different user (or no user) must never see the previous user's cached categories.
    const auth = inject(AuthService);
    let previousUserId = auth.currentUser()?.id ?? null;
    effect(() => {
      const userId = auth.currentUser()?.id ?? null;
      if (userId !== previousUserId) {
        previousUserId = userId;
        this.cache.set([]);
        this.loaded.set(false);
      }
    });
  }

  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(CATEGORIES_URL).pipe(
      tap((categories) => {
        this.cache.set(categories);
        this.loaded.set(true);
      }),
    );
  }

  /** Loads the list once; later calls reuse the cache. */
  ensureLoaded(): void {
    if (!this.loaded()) {
      this.getAll().subscribe({ error: () => undefined });
    }
  }

  create(request: CategoryRequest): Observable<Category[]> {
    return this.http.post<Category>(CATEGORIES_URL, request).pipe(switchMap(() => this.getAll()));
  }

  update(id: number, request: CategoryRequest): Observable<Category[]> {
    return this.http
      .put<Category>(`${CATEGORIES_URL}/${id}`, request)
      .pipe(switchMap(() => this.getAll()));
  }

  /** Soft deletes the category; its tasks become Uncategorized (FR-16). */
  delete(id: number): Observable<Category[]> {
    return this.http.delete<void>(`${CATEGORIES_URL}/${id}`).pipe(switchMap(() => this.getAll()));
  }
}
