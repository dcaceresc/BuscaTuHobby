import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services';
import { MostSearchedProductDto } from '@app/core/models';

@Component({
  selector: 'app-most-searched-products',
  imports: [DecimalPipe],
  templateUrl: './most-searched-products.component.html',
  styleUrl: './most-searched-products.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MostSearchedProductsComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public products = signal<MostSearchedProductDto[]>([]);

  ngOnInit(): void {
    this.loadProducts();
  }

  public loadProducts(): void {
    this.dashboardService.getMostSearchedProducts().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.products.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onViewProduct(productId: string): void {
    this.dashboardService.incrementProductViewCount(productId).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadProducts();
        }
      },
      error: () => {}
    });
  }

  public getBadgeLabel(index: number): string {
    return index < 2 ? 'TRENDING' : 'POPULAR';
  }

  public getBadgeClass(index: number): string {
    return index < 2 ? 'badge-trending' : 'badge-popular';
  }
}
