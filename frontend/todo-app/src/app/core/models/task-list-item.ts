import { Priority } from './priority';

/** A task row in the list (FR-10). Dates are ISO-8601 UTC strings. */
export interface TaskListItem {
  id: number;
  title: string;
  priority: Priority;
  categoryId: number | null;
  categoryName: string | null;
  dueDate: string | null;
  isCompleted: boolean;
  isOverdue: boolean;
  subTaskTotal: number;
  subTaskCompleted: number;
}
