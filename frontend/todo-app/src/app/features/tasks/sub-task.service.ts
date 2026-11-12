import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SetCompletionRequest } from '../../core/models/set-completion-request';
import { SubTask } from '../../core/models/sub-task';
import { SubTaskWriteRequest } from '../../core/models/sub-task-write-request';

const subTasksUrl = (taskId: number): string =>
  `${environment.apiBaseUrl}/tasks/${taskId}/subtasks`;

/** HTTP calls for the checklist items of a task (FR-11, FR-12). */
@Injectable({ providedIn: 'root' })
export class SubTaskService {
  private readonly http = inject(HttpClient);

  create(taskId: number, request: SubTaskWriteRequest): Observable<SubTask> {
    return this.http.post<SubTask>(subTasksUrl(taskId), request);
  }

  update(taskId: number, subTaskId: number, request: SubTaskWriteRequest): Observable<SubTask> {
    return this.http.put<SubTask>(`${subTasksUrl(taskId)}/${subTaskId}`, request);
  }

  setCompletion(taskId: number, subTaskId: number, isCompleted: boolean): Observable<SubTask> {
    const body: SetCompletionRequest = { isCompleted };
    return this.http.patch<SubTask>(`${subTasksUrl(taskId)}/${subTaskId}/complete`, body);
  }

  /** Soft delete (NFR-7). */
  delete(taskId: number, subTaskId: number): Observable<void> {
    return this.http.delete<void>(`${subTasksUrl(taskId)}/${subTaskId}`);
  }
}
