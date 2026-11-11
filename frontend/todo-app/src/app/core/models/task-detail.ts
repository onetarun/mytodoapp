import { SubTask } from './sub-task';
import { TaskListItem } from './task-list-item';

/** A task with all its fields and its checklist. Dates are ISO-8601 UTC strings. */
export interface TaskDetail extends TaskListItem {
  description: string | null;
  completedAt: string | null;
  createdAt: string;
  updatedAt: string;
  subTasks: SubTask[];
}
