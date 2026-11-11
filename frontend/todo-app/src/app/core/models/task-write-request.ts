import { Priority } from './priority';

/** Body of create and update task requests (FR-6, FR-7). `dueDate` is an ISO-8601 UTC string. */
export interface TaskWriteRequest {
  title: string;
  description: string | null;
  dueDate: string | null;
  priority: Priority;
  categoryId: number | null;
}
