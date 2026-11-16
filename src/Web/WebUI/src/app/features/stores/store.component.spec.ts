import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { StoreComponent } from './store.component';
import { routes } from '../../app.routes';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';
import { StoreDetailDto, StoreOfferDto } from '@app/core/models/dashboard/dashboard.model';

const dashboard = { getStoreDetail: vi.fn() };
const notifications = { showError: vi.fn(), showDefaultError: vi.fn() };

function offer(overrides: Partial<StoreOfferDto> = {}): StoreOfferDto {
  return {
    inventoryId: 'a',
    productId: '11111111-1111-1111-1111-111111111111',
    productName: 'RX-78-2 Gundam',
    price: 19990,
    originalPrice: 24990,
    discountPercentage: 20,
    // La API envía la observación sin zona horaria, como la entrega SQL Server.
    observedAtUtc: '2025-09-01T13:45:00',
    condition: 0,
    paymentMethodLabel: null,
    ...overrides
  };
}

function store(overrides: Partial<StoreDetailDto> = {}): StoreDetailDto {
  return {
    storeId: 's1',
    storeName: 'Hobby Store',
    storeSlug: 'hobby-store',
    storeIcon: 'bi bi-shop',
    storeWebSite: 'https://hobby-store.cl',
    productCount: 12,
    offerCount: 2,
    offers: [
      offer(),
      offer({
        inventoryId: 'b',
        productId: '22222222-2222-2222-2222-222222222222',
        productName: 'Zaku II',
        price: 8990,
        originalPrice: 8990,
        discountPercentage: 0,
        observedAtUtc: null,
        condition: 2
      })
    ],
    ...overrides
  };
}

async function render(url = '/stores/hobby-store', data: StoreDetailDto = store()): Promise<RouterTestingHarness> {
  dashboard.getStoreDetail.mockReturnValue(of({ success: true, message: '', data }));
  TestBed.configureTestingModule({
    providers: [
      provideRouter([{ path: 'stores/:slug', component: StoreComponent }]),
      { provide: DashboardService, useValue: dashboard },
      { provide: NotificationService, useValue: notifications }
    ]
  });
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl(url, StoreComponent);
  await harness.fixture.whenStable();
  harness.fixture.detectChanges();
  return harness;
}

describe('StoreComponent', () => {
  beforeEach(() => {
    dashboard.getStoreDetail.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
  });

  it('is declared as a lazy route inside the layout, before the wildcard', async () => {
    const children = routes.find(route => route.path === '')?.children ?? [];
    const index = children.findIndex(child => child.path === 'stores/:slug');
    const wildcard = children.findIndex(child => child.path === '**');

    expect(index).toBeGreaterThan(-1);
    expect(index).toBeLessThan(wildcard);
    const loaded = await children[index].loadComponent!();
    expect(loaded).toBe(StoreComponent);
  });

  it('renders the store header with its icon, website and aggregates', async () => {
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;

    expect(dashboard.getStoreDetail).toHaveBeenCalledWith('hobby-store');
    expect(host.textContent).toContain('Hobby Store');
    expect(host.querySelector('.store__icon')?.className).toContain('bi-shop');
    expect(host.querySelector('.store__website')?.getAttribute('href')).toBe('https://hobby-store.cl');
    expect(host.textContent).toContain('12 productos');
    expect(host.textContent).toContain('2 ofertas');
  });

  it('lists the offers in the order delivered, each one linking to its product', async () => {
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;
    const rows = Array.from(host.querySelectorAll<HTMLElement>('tbody tr'));

    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('RX-78-2 Gundam');
    expect(rows[0].querySelector('a')?.getAttribute('href'))
      .toBe('/products/11111111-1111-1111-1111-111111111111');
    expect(rows[1].textContent).toContain('Zaku II');
    expect(rows[1].querySelector('a')?.getAttribute('href'))
      .toBe('/products/22222222-2222-2222-2222-222222222222');
  });

  it('shows price, normal price, discount, condition and observation date of every offer', async () => {
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;

    const first = host.querySelector<HTMLElement>('tbody tr:nth-child(1)') as HTMLElement;
    const second = host.querySelector<HTMLElement>('tbody tr:nth-child(2)') as HTMLElement;

    expect(first.textContent).toContain('19.990');
    expect(first.querySelector('s')?.textContent).toContain('24.990');
    expect(first.textContent).toContain('-20%');
    expect(first.querySelector('.store__condition')?.textContent).toContain('Nueva');
    expect(first.querySelector('.store__observed')?.textContent).toContain('2025');
    expect(first.querySelector('.store__observed')?.textContent).toContain('13:45');

    expect(second.querySelector('s')).toBeNull();
    expect(second.textContent).not.toContain('-0%');
    expect(second.querySelector('.store__condition')?.textContent).toContain('Usada');
    expect(second.querySelector('.store__observed')?.textContent?.trim()).toBe('');
  });

  it('shows the payment method column with the offer label and leaves it empty when not informed', async () => {
    const harness = await render('/stores/hobby-store', store({
      offers: [
        offer({ paymentMethodLabel: 'Tarjeta CMR' }),
        offer({
          inventoryId: 'b',
          productId: '22222222-2222-2222-2222-222222222222',
          productName: 'Zaku II',
          discountPercentage: 0,
          paymentMethodLabel: null
        })
      ]
    }));
    const host = harness.routeNativeElement as HTMLElement;
    const headers = Array.from(host.querySelectorAll('thead th')).map(th => th.textContent?.trim());

    // El medio de pago entra entre la condición y la observación, sin alterar el orden de las demás columnas.
    expect(headers).toEqual([
      'Producto', 'Precio', 'Normal', 'Descuento', 'Condición', 'Medio de pago', 'Última observación'
    ]);

    const first = host.querySelector<HTMLElement>('tbody tr:nth-child(1)') as HTMLElement;
    const second = host.querySelector<HTMLElement>('tbody tr:nth-child(2)') as HTMLElement;

    expect(first.querySelector('.store__payment')?.textContent).toBe('Tarjeta CMR');
    expect(second.querySelector('.store__payment')?.textContent?.trim()).toBe('');
  });

  it('names the refurbished condition "Reutilizada", the same vocabulary as the product page', async () => {
    const harness = await render('/stores/hobby-store', store({
      offerCount: 1,
      offers: [offer({ condition: 1 })]
    }));
    const host = harness.routeNativeElement as HTMLElement;

    expect(host.querySelector('.store__condition')?.textContent?.trim()).toBe('Reutilizada');
    expect(host.textContent).not.toContain('Refabricada');
  });

  it('shows a Spanish message when the store has no offers', async () => {
    const harness = await render('/stores/hobby-store', store({ offers: [], offerCount: 0 }));
    const text = harness.routeNativeElement?.textContent ?? '';

    expect(text).toContain('Esta tienda todavía no tiene ofertas publicadas.');
    expect(harness.routeNativeElement?.querySelectorAll('tbody tr').length).toBe(0);
  });

  it('reports a failed envelope instead of rendering a blank page', async () => {
    dashboard.getStoreDetail.mockReturnValue(of({
      success: false, message: 'La tienda no existe.', data: null
    }));
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'stores/:slug', component: StoreComponent }]),
        { provide: DashboardService, useValue: dashboard },
        { provide: NotificationService, useValue: notifications }
      ]
    });
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/stores/hobby-store', StoreComponent);
    await harness.fixture.whenStable();
    harness.fixture.detectChanges();

    expect(harness.routeNativeElement?.textContent).toContain('La tienda no existe.');
    expect(notifications.showError).toHaveBeenCalledWith('Error', 'La tienda no existe.');
  });

  it('reports a failed request instead of rendering a blank page', async () => {
    dashboard.getStoreDetail.mockReturnValue(throwError(() => new Error('boom')));
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'stores/:slug', component: StoreComponent }]),
        { provide: DashboardService, useValue: dashboard },
        { provide: NotificationService, useValue: notifications }
      ]
    });
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/stores/hobby-store', StoreComponent);
    await harness.fixture.whenStable();
    harness.fixture.detectChanges();

    expect(harness.routeNativeElement?.textContent).toContain('No pudimos cargar esta tienda.');
    expect(notifications.showDefaultError).toHaveBeenCalled();
  });
});
