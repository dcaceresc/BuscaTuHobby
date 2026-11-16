import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe, registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  StoreDetailDto,
  formatObservedAt,
  offerConditionLabel
} from '@app/core/models/dashboard/dashboard.model';
import { DashboardService, NotificationService } from '@app/core/services';

// Los precios y las fechas se formatean en locale chileno; los datos de locale viajan con este chunk diferido.
registerLocaleData(localeEsCl, 'es-CL');

@Component({
  selector: 'app-store',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './store.component.html',
  styleUrl: './store.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StoreComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);

  public store = signal<StoreDetailDto | null>(null);
  public isLoading = signal(true);
  public errorMessage = signal('');

  // El formateo de la observación es el mismo que usa la ficha de producto.
  public observedAt = formatObservedAt;
  public conditionLabel = offerConditionLabel;

  private slug = '';

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slug = params.get('slug') ?? '';
      this.load();
    });
  }

  private load(): void {
    if (!this.slug) {
      this.isLoading.set(false);
      this.store.set(null);
      this.errorMessage.set('No encontramos esta tienda.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.dashboardService.getStoreDetail(this.slug).subscribe({
      next: (response) => {
        this.isLoading.set(false);

        if (!response.success) {
          this.store.set(null);
          this.errorMessage.set(response.message || 'No pudimos cargar esta tienda.');
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.store.set(response.data);
      },
      error: () => {
        this.isLoading.set(false);
        this.store.set(null);
        this.errorMessage.set('No pudimos cargar esta tienda.');
        this.notificationService.showDefaultError();
      }
    });
  }
}
