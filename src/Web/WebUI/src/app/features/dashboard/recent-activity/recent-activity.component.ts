import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RecentActivityDto } from '@app/core/models';
import { DashboardService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-recent-activity',
  imports: [],
  templateUrl: './recent-activity.component.html',
  styleUrl: './recent-activity.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RecentActivityComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public activities = signal<RecentActivityDto[]>([]);

  ngOnInit(): void {
    this.loadActivities();
  }

  public getTimeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMin < 1) return 'Hace un momento';
    if (diffMin < 60) return `Hace ${diffMin} min`;
    if (diffHours < 24) return `Hace ${diffHours} ${diffHours === 1 ? 'hora' : 'horas'}`;
    return `Hace ${diffDays} ${diffDays === 1 ? 'día' : 'días'}`;
  }

  private loadActivities(): void {
    this.dashboardService.getRecentActivity().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.activities.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }
}
