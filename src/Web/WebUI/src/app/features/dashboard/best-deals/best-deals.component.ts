import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { BestDealDto } from '@app/core/models';
import { DashboardService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-best-deals',
  imports: [DecimalPipe],
  templateUrl: './best-deals.component.html',
  styleUrl: './best-deals.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BestDealsComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public deals = signal<BestDealDto[]>([]);

  ngOnInit(): void {
    this.loadDeals();
  }

  public loadDeals(): void {
    this.dashboardService.getBestDeals().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.deals.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }
}
