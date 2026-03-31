import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FeaturedStoreDto } from '@app/core/models';
import { DashboardService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-featured-stores',
  imports: [DecimalPipe],
  templateUrl: './featured-stores.component.html',
  styleUrl: './featured-stores.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FeaturedStoresComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public stores = signal<FeaturedStoreDto[]>([]);

  ngOnInit(): void {
    this.loadStores();
  }

  private loadStores(): void {
    this.dashboardService.getFeaturedStores().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.stores.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }
}
