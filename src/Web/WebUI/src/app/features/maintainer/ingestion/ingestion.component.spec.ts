import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { of, Subject } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { IngestionComponent } from './ingestion.component';
import { routes as maintainerRoutes } from '../maintainer.routes';
import { routes as ingestionRoutes } from './ingestion.routes';
import { AdminGuard } from '../../../core/guards/admin.guard';
import { SuperAdminGuard } from '../../../core/guards/super-admin.guard';
import { AdministrationMenuComponent } from '@app/shared';
import { AuthService, IngestionService, NotificationService } from '@app/core/services';
import { ApiResponse, IngestionRunDto, IngestionStatusDto } from '@app/core/models';
import { registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { NgbActiveOffcanvas } from '@ng-bootstrap/ng-bootstrap/offcanvas';

// La fecha de la última ejecución se formatea en es-CL: el spec necesita los datos de ese locale, igual que la app.
registerLocaleData(localeEsCl, 'es-CL');

// El estado de una ejecución viaja como entero: 0 Running, 1 Completed, 2 Failed (enum ImportRunStatus).
const run = (overrides: Partial<IngestionRunDto> = {}): IngestionRunDto => ({
  importRunId: 'run-1',
  startedAtUtc: '2026-11-16T12:00:00',
  finishedAtUtc: '2026-11-16T12:05:00',
  status: 1,
  requestBudget: 50,
  discoveredCount: 20,
  storedCount: 12,
  skippedCount: 7,
  failedCount: 1,
  ...overrides
});

const store = (overrides: Partial<IngestionStatusDto> = {}): IngestionStatusDto => ({
  storeId: 'store-1',
  storeName: 'Irion',
  storeSlug: 'irion',
  connectorKey: 'shopify',
  isActive: true,
  totalProducts: 22,
  coveredProducts: 10,
  coveragePercentage: 45,
  offersObservedLast24Hours: 3,
  offersOlderThanSevenDays: 1,
  lastRun: run(),
  ...overrides
});

const ingestion = { getStatus: vi.fn(), startRun: vi.fn() };
const notifications = { showSuccess: vi.fn(), showError: vi.fn(), showDefaultError: vi.fn() };

async function render() {
  TestBed.configureTestingModule({
    imports: [IngestionComponent],
    providers: [
      { provide: IngestionService, useValue: ingestion },
      { provide: NotificationService, useValue: notifications }
    ]
  });
  const fixture = TestBed.createComponent(IngestionComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture;
}

function rows(fixture: { nativeElement: HTMLElement }): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll('tbody tr')) as HTMLElement[];
}

describe('IngestionComponent', () => {
  beforeEach(() => {
    ingestion.getStatus.mockReset();
    ingestion.startRun.mockReset();
    notifications.showSuccess.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
    ingestion.getStatus.mockReturnValue(of({
      success: true,
      message: '',
      data: [
        store(),
        store({ storeId: 'store-2', storeName: 'LuffyToys', storeSlug: 'luffytoys', connectorKey: 'shopify',
          coveragePercentage: 0, coveredProducts: 0, offersObservedLast24Hours: 0, offersOlderThanSevenDays: 0,
          lastRun: null })
      ]
    }));
  });

  it('renders one row per configured store with connector, coverage, freshness and last run', async () => {
    const fixture = await render();
    const rendered = rows(fixture);

    expect(rendered).toHaveLength(2);
    expect(rendered[0].textContent).toContain('Irion');
    expect(rendered[0].textContent).toContain('shopify');
    expect(rendered[0].textContent).toContain('45%');
    expect(rendered[0].textContent).toContain('10 de 22 productos');
    expect(rendered[0].textContent).toContain('Últimas 24 h: 3');
    expect(rendered[0].textContent).toContain('Más de 7 días: 1');
    expect(rendered[0].textContent).toContain('Completada');
    expect(rendered[0].textContent).toContain('Guardadas 12');
    expect(rendered[1].textContent).toContain('Sin ejecuciones');
  });

  it('hides the run button while the last run of that store is still open', async () => {
    ingestion.getStatus.mockReturnValue(of({
      success: true,
      message: '',
      data: [store({ lastRun: run({ status: 0, finishedAtUtc: null }) }),
        store({ storeId: 'store-2', storeName: 'LuffyToys', storeSlug: 'luffytoys' })]
    }));
    const fixture = await render();
    const rendered = rows(fixture);

    expect(rendered[0].querySelector('button')).toBeNull();
    expect(rendered[0].textContent).toContain('Ejecución en curso');
    expect(rendered[1].querySelector('button')?.textContent).toContain('Ejecutar ahora');
  });

  it('renders an empty state when no store has ingestion configured', async () => {
    ingestion.getStatus.mockReturnValue(of({ success: true, message: '', data: [] }));
    const fixture = await render();

    expect(rows(fixture)).toHaveLength(0);
    expect(fixture.nativeElement.textContent).toContain('No hay tiendas con ingesta configurada');
    expect(notifications.showError).not.toHaveBeenCalled();
  });

  it('reports a failed envelope with the backend message instead of rendering rows', async () => {
    ingestion.getStatus.mockReturnValue(of({
      success: false, message: 'Ha ocurrido un error al obtener el estado de la ingesta.', data: null!
    }));
    const fixture = await render();

    expect(notifications.showError).toHaveBeenCalledExactlyOnceWith('Ingesta',
      'Ha ocurrido un error al obtener el estado de la ingesta.');
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent)
      .toContain('Ha ocurrido un error al obtener el estado de la ingesta.');
    expect(rows(fixture)).toHaveLength(0);
  });

  it('posts only the store slug when the budget field stays empty', async () => {
    ingestion.startRun.mockReturnValue(of({ success: true, message: '', data: run() }));
    const fixture = await render();

    rows(fixture)[0].querySelector('button')!.click();
    await fixture.whenStable();

    expect(ingestion.startRun).toHaveBeenCalledExactlyOnceWith({ storeSlug: 'irion' });
    expect(ingestion.startRun.mock.calls[0][0]).not.toHaveProperty('requestBudget');
  });

  it('posts the typed budget, disables the button in flight and shows what the run stored', async () => {
    const response = new Subject<ApiResponse<IngestionRunDto>>();
    ingestion.startRun.mockReturnValue(response);
    const fixture = await render();
    const row = rows(fixture)[0];
    const input = row.querySelector('input') as HTMLInputElement;
    input.value = '25';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    row.querySelector('button')!.click();
    await fixture.whenStable();

    expect(ingestion.startRun).toHaveBeenCalledExactlyOnceWith({ storeSlug: 'irion', requestBudget: 25 });
    expect((rows(fixture)[0].querySelector('button') as HTMLButtonElement).disabled).toBe(true);

    response.next({ success: true, message: '', data: run({ storedCount: 9 }) });
    response.complete();
    await fixture.whenStable();

    expect((rows(fixture)[0].querySelector('button') as HTMLButtonElement).disabled).toBe(false);
    expect(notifications.showSuccess).toHaveBeenCalledExactlyOnceWith('Ingesta',
      'Ejecución de Irion: 9 ofertas guardadas, 7 omitidas y 1 fallidas.');
    expect(rows(fixture)[0].textContent).toContain('Guardadas 9');
  });

  it('rejects a non-positive budget with a message and without posting', async () => {
    const fixture = await render();
    const row = rows(fixture)[0];
    const input = row.querySelector('input') as HTMLInputElement;
    input.value = '0';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    row.querySelector('button')!.click();
    await fixture.whenStable();

    expect(ingestion.startRun).not.toHaveBeenCalled();
    expect(notifications.showError).toHaveBeenCalledWith('Ingesta', expect.stringContaining('entero positivo'));
  });

  it('keeps the screen alive when the trigger fails or the transport fails', async () => {
    ingestion.startRun.mockReturnValue(of({
      success: false, message: 'La tienda irion ya tiene una ingesta abierta.', data: null!
    }));
    const fixture = await render();

    rows(fixture)[0].querySelector('button')!.click();
    await fixture.whenStable();

    expect(notifications.showError).toHaveBeenCalledWith('Ingesta', 'La tienda irion ya tiene una ingesta abierta.');
    expect((rows(fixture)[0].querySelector('button') as HTMLButtonElement).disabled).toBe(false);

    const failure = new Subject<ApiResponse<IngestionRunDto>>();
    ingestion.startRun.mockReturnValue(failure);
    rows(fixture)[0].querySelector('button')!.click();
    await fixture.whenStable();
    failure.error(new Error('offline'));
    await fixture.whenStable();

    expect(notifications.showDefaultError).toHaveBeenCalled();
    expect((rows(fixture)[0].querySelector('button') as HTMLButtonElement).disabled).toBe(false);
  });
});

describe('Ingestion routes and navigation', () => {
  it('guards the ingestion screen with AdminGuard because the API accepts SuperAdmin or Administrator', () => {
    expect(ingestionRoutes[0].canActivate).toEqual([AdminGuard]);
    expect(ingestionRoutes[0].canActivate).not.toContain(SuperAdminGuard);
    expect(ingestionRoutes[0].loadComponent).toBeDefined();
  });

  it('keeps the lazy child declaration in the maintainer shell', () => {
    const route = maintainerRoutes.find(candidate => candidate.path === 'ingestion');

    expect(route?.loadChildren).toBeDefined();
    expect(maintainerRoutes.find(candidate => candidate.path === 'products')?.loadChildren).toBeDefined();
  });

  it('links the ingestion screen from the administration menu', async () => {
    await TestBed.configureTestingModule({
      imports: [AdministrationMenuComponent],
      providers: [provideRouter([]),
        { provide: AuthService, useValue: { isSuperAdmin: () => false } },
        { provide: NgbActiveOffcanvas, useValue: { dismiss: vi.fn() } }]
    }).compileComponents();
    const fixture = TestBed.createComponent(AdministrationMenuComponent);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('a[href="/maintainer/ingestion"]').textContent).toContain('Ingesta');
    expect(fixture.nativeElement.querySelector('a[href="/maintainer/stores"]')).not.toBeNull();
  });
});

describe('IngestionService', () => {
  it('reads the status and posts the run body to the published routes', () => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    });
    const http = TestBed.inject(HttpTestingController);
    const service = TestBed.inject(IngestionService);
    let status: ApiResponse<IngestionStatusDto[]> | undefined;
    let started: ApiResponse<IngestionRunDto> | undefined;

    service.getStatus().subscribe(value => status = value);
    const statusRequest = http.expectOne('/api/maintainer/ingestion/status');
    expect(statusRequest.request.method).toBe('GET');
    statusRequest.flush({ success: true, message: '', data: [store()] });

    service.startRun({ storeSlug: 'irion', requestBudget: 10 }).subscribe(value => started = value);
    const runRequest = http.expectOne('/api/maintainer/ingestion/runs');
    expect(runRequest.request.method).toBe('POST');
    expect(runRequest.request.body).toEqual({ storeSlug: 'irion', requestBudget: 10 });
    runRequest.flush({ success: true, message: '', data: run() });

    expect(status?.data).toHaveLength(1);
    expect(started?.data?.storedCount).toBe(12);
    http.verify();
  });
});
