import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { PriceDropDto, formatObservedAt } from '@app/core/models';
import { DashboardService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-price-drops',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './price-drops.component.html',
  styleUrl: './price-drops.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PriceDropsComponent implements OnInit {

  private readonly dashboardService = inject(DashboardService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  public readonly drops = signal<PriceDropDto[]>([]);
  public readonly loading = signal(false);
  public readonly error = signal('');

  public ngOnInit(): void {
    this.loadDrops();
  }

  /** La ventana y el umbral viajan explícitos: la API los acota igual que la ruta de historial de precios. */
  public loadDrops(): void {
    this.loading.set(true);
    this.error.set('');
    this.dashboardService.getPriceDrops(30, 10).pipe(takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))).subscribe({
      next: response => {
        if (!response.success || !response.data) {
          this.drops.set([]);
          this.fail(response.message || 'No se pudieron cargar las bajadas de precio.');
          return;
        }
        this.drops.set(response.data);
      },
      error: () => {
        this.drops.set([]);
        this.error.set('No se pudieron cargar las bajadas de precio.');
        this.notifications.showDefaultError();
      }
    });
  }

  /** La observación se muestra con el mismo formato es-CL que el resto de las fechas de oferta. */
  public formatDate(value: string): string {
    return formatObservedAt(value);
  }

  private fail(message: string): void {
    this.error.set(message);
    this.notifications.showError('Bajadas de precio', message);
  }
}
