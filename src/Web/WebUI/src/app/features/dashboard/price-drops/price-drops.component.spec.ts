import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { of, Subject } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { PriceDropsComponent } from './price-drops.component';
import { DashboardService, NotificationService } from '@app/core/services';
import { ApiResponse, PriceDropDto, formatObservedAt } from '@app/core/models';
import { registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';

// Los precios y las fechas se formatean en es-CL: el spec necesita los datos de ese locale, igual que la app.
registerLocaleData(localeEsCl, 'es-CL');

const dashboard = { getPriceDrops: vi.fn() };
const notifications = { showSuccess: vi.fn(), showError: vi.fn(), showDefaultError: vi.fn() };

const drop = (overrides: Partial<PriceDropDto> = {}): PriceDropDto => ({
  inventoryId: 'i1',
  productId: 'p1',
  productName: 'RX-78-2 Gundam',
  storeName: 'Irion',
  storeSlug: 'irion',
  sourceUrl: 'https://irion.cl/products/rx-78-2',
  currentPrice: 19990,
  previousPrice: 24990,
  dropPercentage: 20,
  observedAtUtc: '2026-11-16T12:00:00',
  previousObservedAtUtc: '2026-11-10T12:00:00',
  ...overrides
});

async function render() {
  TestBed.configureTestingModule({
    imports: [PriceDropsComponent],
    providers: [
      provideRouter([]),
      { provide: DashboardService, useValue: dashboard },
      { provide: NotificationService, useValue: notifications }
    ]
  });
  const fixture = TestBed.createComponent(PriceDropsComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture;
}

function rows(fixture: { nativeElement: HTMLElement }): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll('tbody tr')) as HTMLElement[];
}

describe('PriceDropsComponent', () => {
  beforeEach(() => {
    dashboard.getPriceDrops.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
    dashboard.getPriceDrops.mockReturnValue(of({
      success: true,
      message: '',
      data: [drop(), drop({ inventoryId: 'i2', productId: 'p2', productName: 'Unicorn', storeName: 'LuffyToys',
        dropPercentage: 12, currentPrice: 31990, previousPrice: 36490 })]
    }));
  });

  it('renders one row per price drop and links the product to its page', async () => {
    const fixture = await render();
    const rendered = rows(fixture);

    expect(rendered).toHaveLength(2);
    expect(rendered[0].textContent).toContain('RX-78-2 Gundam');
    expect(rendered[0].textContent).toContain('Irion');
    expect(rendered[0].textContent).toContain('19.990');
    expect(rendered[0].textContent).toContain('-20%');
    expect(rendered[1].textContent).toContain('LuffyToys');
    expect(rendered[1].textContent).toContain('-12%');
    expect(rendered[0].querySelector('a')!.getAttribute('href')).toBe('/products/p1');
    expect(rendered[1].querySelector('a')!.getAttribute('href')).toBe('/products/p2');
  });

  it('shows the previous price struck through', async () => {
    const fixture = await render();
    const struck = rows(fixture)[0].querySelector('s');

    expect(struck?.textContent).toContain('24.990');
    expect(struck?.textContent).not.toContain('19.990');
  });

  it('shows the observation date in es-CL instead of the raw API value', async () => {
    const fixture = await render();
    const text = rows(fixture)[0].textContent!;

    expect(text).toContain(formatObservedAt('2026-11-16T12:00:00'));
    expect(text).not.toContain('2026-11-16T12:00:00');
  });

  it('renders a clear empty state because today every offer has a single history point', async () => {
    dashboard.getPriceDrops.mockReturnValue(of({ success: true, message: '', data: [] }));
    const fixture = await render();

    expect(rows(fixture)).toHaveLength(0);
    expect(fixture.nativeElement.textContent).toContain('Todavía no hay bajadas de precio registradas');
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
    expect(notifications.showError).not.toHaveBeenCalled();
  });

  it('renders an error state inside the card without breaking the dashboard', async () => {
    dashboard.getPriceDrops.mockReturnValue(of({
      success: false, message: 'Ha ocurrido un error al obtener las bajadas de precio.', data: null!
    }));
    const fixture = await render();

    expect(notifications.showError).toHaveBeenCalledExactlyOnceWith('Bajadas de precio',
      'Ha ocurrido un error al obtener las bajadas de precio.');
    expect(fixture.nativeElement.querySelector('.card')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent)
      .toContain('Ha ocurrido un error al obtener las bajadas de precio.');
    expect(rows(fixture)).toHaveLength(0);
  });

  it('asks the API for the default window and threshold', async () => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    });
    const http = TestBed.inject(HttpTestingController);
    const service = TestBed.inject(DashboardService);
    let received: ApiResponse<PriceDropDto[]> | undefined;

    service.getPriceDrops().subscribe(value => received = value);
    const request = http.expectOne('api/dashboard/price-drops?days=30&minPercent=10');
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, message: '', data: [drop()] });

    expect(received?.data).toHaveLength(1);
    http.verify();
  });
});

describe('PriceDropsComponent transport failure', () => {
  it('reports a failed transport inside the card without breaking the dashboard', async () => {
    const response = new Subject<ApiResponse<PriceDropDto[]>>();
    dashboard.getPriceDrops.mockReset();
    notifications.showDefaultError.mockReset();
    dashboard.getPriceDrops.mockReturnValue(response);

    const fixture = await render();
    response.error(new Error('offline'));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(notifications.showDefaultError).toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('.card')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
    expect(rows(fixture)).toHaveLength(0);
  });
});
