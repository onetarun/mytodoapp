import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SetCompletionRequest } from '../../core/models/set-completion-request';
import { TaskDetail } from '../../core/models/task-detail';
import { TaskListItem } from '../../core/models/task-list-item';
import { TaskStatusFilter } from '../../core/models/task-status-filter';
import { TaskWriteRequest } from '../../core/models/task-write-request';

const TASKS_URL = `${environment.apiBaseUrl}/tasks`;

/** HTTP calls for the current user's tasks (FR-6 to FR-10a). */
@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);

  getAll(status: TaskStatusFilter): Observable<TaskListItem[]> {
    return this.http.get<TaskListItem[]>(TASKS_URL, {
      params: new HttpParams().set('status', status),
    });
  }

  getById(id: number): Observable<TaskDetail> {
    return this.http.get<TaskDetail>(`${TASKS_URL}/${id}`);
  }

  create(request: TaskWriteRequest): Observable<TaskDetail> {
    return this.http.post<TaskDetail>(TASKS_URL, request);
  }

  update(id: number, request: TaskWriteRequest): Observable<TaskDetail> {
    return this.http.put<TaskDetail>(`${TASKS_URL}/${id}`, request);
  }

  setCompletion(id: number, isCompleted: boolean): Observable<TaskDetail> {
    const body: SetCompletionRequest = { isCompleted };
    return this.http.patch<TaskDetail>(`${TASKS_URL}/${id}/complete`, body);
  }

  /** Soft delete (FR-8, NFR-7). */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${TASKS_URL}/${id}`);
  }
}
