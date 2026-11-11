import { NotificationType } from './notification-type';

/** One task in the notification bell (FR-19, FR-20). Named to avoid the DOM `Notification` type. */
export interface TaskNotification {
  taskId: number;
  title: string;
  dueDate: string;
  type: NotificationType;
}
