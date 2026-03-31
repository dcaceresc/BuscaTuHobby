import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe, UpperCasePipe } from '@angular/common';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services';
import { RecentPostDto } from '@app/core/models';

@Component({
  selector: 'app-recent-posts',
  imports: [DecimalPipe, UpperCasePipe],
  templateUrl: './recent-posts.component.html',
  styleUrl: './recent-posts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RecentPostsComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public posts = signal<RecentPostDto[]>([]);

  ngOnInit(): void {
    this.loadRecentPosts();
  }

  public loadRecentPosts(): void {
    this.dashboardService.getRecentPosts().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.posts.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onReadMore(postId: string): void {
    this.dashboardService.incrementPostViewCount(postId).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadRecentPosts();
        }
      },
      error: () => {}
    });
  }

  public getBadgeClass(postTypeName: string): string {
    const name = postTypeName.toLowerCase();
    if (name === 'guia') return 'bg-success';
    if (name === 'comparativa') return 'bg-warning text-dark';
    if (name === 'top') return 'bg-danger';
    return 'bg-secondary';
  }
}
