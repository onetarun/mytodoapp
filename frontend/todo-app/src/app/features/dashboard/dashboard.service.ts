import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { DashboardSummary } from '../../core/models/dashboard-summary';

const SUMMARY_URL = `${environment.apiBaseUrl}/dashboard/summary`;

/** HTTP call for the dashboard counts and upcoming tasks (FR-17, FR-18). */
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  /** Sends the browser time zone so "Due Today" uses the user's local day. */
  getSummary(): Observable<DashboardSummary> {
    const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    return this.http.get<DashboardSummary>(SUMMARY_URL, {
      params: new HttpParams().set('tz', timeZone),
    });
  }
}
