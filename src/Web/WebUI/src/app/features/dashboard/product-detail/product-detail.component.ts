import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe, formatDate, registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';
import { ProductDetailDto, ProductPricePointDto } from '@app/core/models/dashboard/dashboard.model';
import { PriceChartComponent } from '@app/shared/components/price-chart/price-chart.component';

// Los precios y las fechas se formatean en locale chileno; los datos de locale viajan con este chunk diferido.
registerLocaleData(localeEsCl, 'es-CL');

// Ventanas de días que ofrece el lector; 0 pide la serie completa a la API.
const PRICE_WINDOWS = [
  { label: '30 días', days: 30 },
  { label: '90 días', days: 90 },
  { label: 'Todo', days: 0 }
] as const;

// La condición de la oferta llega como entero desde la API: 0 nueva, 1 reutilizada, 2 usada.
const CONDITION_LABELS: Record<number, string> = { 0: 'Nueva', 1: 'Reutilizada', 2: 'Usada' };
const CONDITION_BADGES: Record<number, string> = {
  0: 'bg-success',
  1: 'bg-warning text-dark',
  2: 'bg-secondary'
};

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
  public isWindowLoading = signal(false);
  public notFound = signal(false);
  public selectedImage = signal(0);
  public readonly priceWindows = PRICE_WINDOWS;
  public selectedDays = signal(0);

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

  /** Cambia la ventana del historial y recarga solo la serie, no la página. */
  public selectWindow(days: number): void {
    if (this.selectedDays() === days) return;
    this.selectedDays.set(days);
    this.loadPriceHistory(days, true);
  }

  public conditionLabel(condition: number): string {
    return CONDITION_LABELS[condition] ?? 'Sin dato';
  }

  public conditionClass(condition: number): string {
    return CONDITION_BADGES[condition] ?? 'bg-light text-dark';
  }

  /**
   * La API envía la observación sin zona horaria; como el campo se llama `observedAtUtc` se interpreta como UTC
   * para que la fecha mostrada no se desplace con la zona del navegador.
   */
  public observedAt(value: string): string {
    const iso = /(Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}Z`;
    return formatDate(iso, 'd MMM y, HH:mm', 'es-CL', 'UTC');
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

  private loadPriceHistory(days = 0, isWindowChange = false): void {
    // Un cambio de ventana conserva el gráfico anterior mientras llega la serie nueva.
    if (isWindowChange) {
      this.isWindowLoading.set(true);
    } else {
      this.isChartLoading.set(true);
    }
    this.dashboardService.getProductPriceHistory(this.productId, days).subscribe({
      next: (response) => {
        this.isChartLoading.set(false);
        this.isWindowLoading.set(false);
        if (!response.success) return;
        this.pricePoints.set(response.data.points);
      },
      error: () => {
        this.isChartLoading.set(false);
        this.isWindowLoading.set(false);
      }
    });
  }
}
