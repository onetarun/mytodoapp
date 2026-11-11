import { Routes } from '@angular/router';

export const TASKS_ROUTES: Routes = [
  {
    path: '',
    title: 'Tasks',
    loadComponent: () =>
      import('./task-list-page/task-list-page.component').then((m) => m.TaskListPageComponent),
  },
  {
    path: ':id',
    title: 'Task',
    loadComponent: () =>
      import('./task-detail-page/task-detail-page.component').then(
        (m) => m.TaskDetailPageComponent,
      ),
  },
];
