import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { IngestionRunDto, IngestionStatusDto, StartIngestionRun, formatIngestionDate, importRunStatusLabel, isImportRunOpen } from '@app/core/models';
import { IngestionService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-ingestion',
  imports: [],
  templateUrl: './ingestion.component.html',
  styleUrl: './ingestion.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IngestionComponent implements OnInit {

  private readonly ingestionService = inject(IngestionService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  public readonly stores = signal<IngestionStatusDto[]>([]);
  public readonly loading = signal(false);
  public readonly error = signal('');
  // El fallo de un disparo no puede ocultar la tabla: el operador necesita el mensaje y las filas a la vez.
  public readonly runError = signal('');
  // Presupuesto escrito por el operador, por slug. Vacío significa "usa el de la tienda".
  public readonly budgets = signal<Record<string, string>>({});
  // Ejecución devuelta por el último disparo: describe lo que la pasada guardó, no lo que el estado leyó antes.
  public readonly lastRuns = signal<Record<string, IngestionRunDto>>({});
  // Ejecuciones en vuelo por slug, para apagar el botón mientras la petición está abierta.
  public readonly running = signal<Record<string, boolean>>({});

  public ngOnInit(): void {
    this.loadStatus();
  }

  public loadStatus(): void {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    this.ingestionService.getStatus().pipe(takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))).subscribe({
      next: response => {
        if (!response.success || !response.data) {
          this.stores.set([]);
          this.fail(response.message || 'No se pudo obtener el estado de la ingesta.');
          return;
        }
        this.stores.set(response.data);
      },
      error: () => {
        this.stores.set([]);
        this.error.set('No se pudo obtener el estado de la ingesta.');
        this.notifications.showDefaultError();
      }
    });
  }

  /** Última ejecución visible de la tienda: la que devolvió el disparo o, si no hubo, la que publicó el estado. */
  public currentRun(store: IngestionStatusDto): IngestionRunDto | null {
    return this.lastRuns()[store.storeSlug] ?? store.lastRun;
  }

  public isOpen(store: IngestionStatusDto): boolean {
    return isImportRunOpen(this.currentRun(store));
  }

  public isRunning(storeSlug: string): boolean {
    return this.running()[storeSlug] === true;
  }

  public budget(storeSlug: string): string {
    return this.budgets()[storeSlug] ?? '';
  }

  public onBudget(storeSlug: string, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.budgets.update(current => ({ ...current, [storeSlug]: value }));
  }

  public statusLabel(status: number): string {
    return importRunStatusLabel(status);
  }

  public formatRunDate(value: string | null): string {
    return formatIngestionDate(value);
  }

  public run(store: IngestionStatusDto): void {
    if (this.isRunning(store.storeSlug) || this.isOpen(store)) return;

    const request: StartIngestionRun = { storeSlug: store.storeSlug };
    const raw = this.budget(store.storeSlug).trim();
    if (raw !== '') {
      // El presupuesto manual sólo recorta el configurado, así que un valor que no describe una petición se rechaza
      // aquí en vez de mandarlo para que la API lo adivine.
      if (!/^\d+$/.test(raw) || Number(raw) < 1) {
        this.failRun('El presupuesto debe ser un número entero positivo; déjalo vacío para usar el de la tienda.');
        return;
      }
      request.requestBudget = Number(raw);
    }

    this.error.set('');
    this.runError.set('');
    this.setRunning(store.storeSlug, true);
    this.ingestionService.startRun(request).pipe(takeUntilDestroyed(this.destroyRef),
      finalize(() => this.setRunning(store.storeSlug, false))).subscribe({
      next: response => {
        const data = response.data;
        if (!response.success || !data) {
          this.failRun(response.message || 'No se pudo ejecutar la ingesta.');
          return;
        }
        this.lastRuns.update(current => ({ ...current, [store.storeSlug]: data }));
        // El aviso habla de lo que la ejecución dejó escrito, no de lo que se pidió.
        this.notifications.showSuccess('Ingesta',
          `Ejecución de ${store.storeName}: ${data.storedCount} ofertas guardadas, ${data.skippedCount} omitidas y ${data.failedCount} fallidas.`);
      },
      error: () => {
        this.runError.set('No se pudo ejecutar la ingesta.');
        this.notifications.showDefaultError();
      }
    });
  }

  private setRunning(storeSlug: string, value: boolean): void {
    this.running.update(current => ({ ...current, [storeSlug]: value }));
  }

  private fail(message: string): void {
    this.error.set(message);
    this.notifications.showError('Ingesta', message);
  }

  private failRun(message: string): void {
    this.runError.set(message);
    this.notifications.showError('Ingesta', message);
  }
}
