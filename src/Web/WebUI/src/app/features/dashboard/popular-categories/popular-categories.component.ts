import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { PopularCategoryDto } from '@app/core/models';
import { DashboardService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-popular-categories',
  imports: [],
  templateUrl: './popular-categories.component.html',
  styleUrl: './popular-categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PopularCategoriesComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public categories = signal<PopularCategoryDto[]>([]);

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.dashboardService.getPopularCategories().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.categories.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }
}
