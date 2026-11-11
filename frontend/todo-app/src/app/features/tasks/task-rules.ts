import { Priority } from '../../core/models/priority';
import { TaskStatusFilter } from '../../core/models/task-status-filter';

/** Task field limits and choices; they mirror the API (`TodoTask.TitleMaxLength`, `DescriptionMaxLength`). */
export const TASK_RULES = {
  titleMaxLength: 200,
  descriptionMaxLength: 2000,
  defaultPriority: 'Medium' as Priority,
  priorities: ['High', 'Medium', 'Low'] as const satisfies readonly Priority[],
  statusFilters: ['all', 'pending', 'completed'] as const satisfies readonly TaskStatusFilter[],
};

/** Reads the `?status=` query value; anything unknown falls back to `all` (FR-10a default). */
export function parseStatusFilter(value: string | undefined): TaskStatusFilter {
  return TASK_RULES.statusFilters.find((status) => status === value) ?? 'all';
}
