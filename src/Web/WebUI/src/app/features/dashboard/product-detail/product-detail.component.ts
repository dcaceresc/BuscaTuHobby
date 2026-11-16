import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe, registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';
import { ProductDetailDto, ProductPricePointDto } from '@app/core/models/dashboard/dashboard.model';
import { PriceChartComponent } from '@app/shared/components/price-chart/price-chart.component';

// Los precios y las fechas se formatean en locale chileno; los datos de locale viajan con este chunk diferido.
registerLocaleData(localeEsCl, 'es-CL');

@Component({
  selector: 'app-product-detail',
  imports: [DatePipe, DecimalPipe, RouterLink, PriceChartComponent],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDetailComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);

  public product = signal<ProductDetailDto | null>(null);
  public pricePoints = signal<ProductPricePointDto[]>([]);
  public isLoading = signal(true);
  public isChartLoading = signal(true);
  public notFound = signal(false);
  public selectedImage = signal(0);

  /** La galería recurre al placeholder compartido cuando el producto no tiene imágenes. */
  public mainImage = computed(() => {
    const images = this.product()?.imageUrls ?? [];
    return images.length === 0 ? 'assets/images/placeholder.svg' : images[this.selectedImage()] ?? images[0];
  });

  private productId = '';

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('productId') ?? '';
    if (!this.productId) {
      this.isLoading.set(false);
      this.isChartLoading.set(false);
      this.notFound.set(true);
      return;
    }
    this.loadProduct();
    this.loadPriceHistory();
  }

  public selectImage(index: number): void {
    this.selectedImage.set(index);
  }

  private loadProduct(): void {
    this.dashboardService.getProductDetail(this.productId).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (!response.success) {
          this.notFound.set(true);
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.product.set(response.data);
        this.dashboardService.incrementProductViewCount(this.productId).subscribe({ error: () => {} });
      },
      error: () => {
        this.isLoading.set(false);
        this.notFound.set(true);
        this.notificationService.showDefaultError();
      }
    });
  }

  private loadPriceHistory(): void {
    this.dashboardService.getProductPriceHistory(this.productId).subscribe({
      next: (response) => {
        this.isChartLoading.set(false);
        if (!response.success) return;
        this.pricePoints.set(response.data.points);
      },
      error: () => {
        this.isChartLoading.set(false);
      }
    });
  }
}
