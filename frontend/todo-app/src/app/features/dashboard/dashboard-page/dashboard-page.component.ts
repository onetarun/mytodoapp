import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { DashboardSummary } from '../../../core/models/dashboard-summary';
import { TaskStatusFilter } from '../../../core/models/task-status-filter';
import { TaskWriteRequest } from '../../../core/models/task-write-request';
import { ToastService } from '../../../core/ui/toast.service';
import { PriorityBadgeComponent } from '../../../shared/priority-badge/priority-badge.component';
import { TaskFormComponent } from '../../tasks/task-form/task-form.component';
import { TaskService } from '../../tasks/task.service';
import { DashboardService } from '../dashboard.service';

/** One stat card; `status` is set when the task list has a matching filter. */
interface StatCard {
  key: 'total' | 'completed' | 'pending' | 'overdue' | 'dueToday';
  label: string;
  icon: string;
  status: TaskStatusFilter | null;
  danger: boolean;
}

const STAT_CARDS: readonly StatCard[] = [
  { key: 'total', label: 'Total', icon: 'list', status: 'all', danger: false },
  { key: 'completed', label: 'Completed', icon: 'task_alt', status: 'completed', danger: false },
  { key: 'pending', label: 'Pending', icon: 'pending_actions', status: 'pending', danger: false },
  { key: 'overdue', label: 'Overdue', icon: 'warning', status: null, danger: true },
  { key: 'dueToday', label: 'Due Today', icon: 'today', status: null, danger: false },
];

/**
 * Dashboard (FR-17, FR-18, US-7): five stat cards, the upcoming tasks for the next 7 days
 * and a quick-add box (NFR-21). A brand-new user sees an empty state instead of the stats.
 */
@Component({
  selector: 'app-dashboard-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    NgTemplateOutlet,
    PriorityBadgeComponent,
    RouterLink,
    TaskFormComponent,
  ],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPageComponent {
  private readonly dashboardService = inject(DashboardService);
  private readonly taskService = inject(TaskService);
  private readonly toast = inject(ToastService);

  protected readonly cards = STAT_CARDS;
  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadFailed = signal(false);
  protected readonly isEmpty = computed(() => this.summary()?.total === 0);

  protected readonly createTask = (request: TaskWriteRequest) => this.taskService.create(request);

  constructor() {
    this.load();
  }

  protected onCreated(): void {
    this.toast.success('Task added.');
    this.load();
  }

  /** `?status=` for the card link; `all` is the list default, so it needs no parameter. */
  protected statusParams(status: TaskStatusFilter): Record<string, string> {
    return status === 'all' ? {} : { status };
  }

  protected load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.dashboardService
      .getSummary()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (summary) => this.summary.set(summary),
        error: () => this.loadFailed.set(true),
      });
  }
}
