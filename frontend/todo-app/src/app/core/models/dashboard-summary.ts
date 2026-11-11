import { TaskListItem } from './task-list-item';

/** Dashboard counts (FR-17) and upcoming tasks for the next 7 days (FR-18). */
export interface DashboardSummary {
  total: number;
  completed: number;
  pending: number;
  overdue: number;
  dueToday: number;
  upcoming: TaskListItem[];
}
