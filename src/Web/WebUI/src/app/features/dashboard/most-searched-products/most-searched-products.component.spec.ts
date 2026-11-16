import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MostSearchedProductsComponent } from './most-searched-products.component';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';
import { MostSearchedProductDto } from '@app/core/models/dashboard/dashboard.model';

function product(overrides: Partial<MostSearchedProductDto> = {}): MostSearchedProductDto {
  return {
    productId: '11111111-1111-1111-1111-111111111111',
    productName: 'RX-78-2 Gundam',
    productDescription: 'Maqueta a escala',
    productViewCount: 1234,
    storeCount: 2,
    storeNames: ['Tienda Uno', 'Tienda Dos'],
    ...overrides
  };
}

const dashboard = {
  getMostSearchedProducts: vi.fn(),
  incrementProductViewCount: vi.fn()
};
const notifications = { showError: vi.fn(), showDefaultError: vi.fn() };

async function render(products = [product()]) {
  TestBed.configureTestingModule({
    imports: [MostSearchedProductsComponent],
    providers: [
      provideRouter([]),
      { provide: DashboardService, useValue: dashboard },
      { provide: NotificationService, useValue: notifications }
    ]
  });
  const fixture = TestBed.createComponent(MostSearchedProductsComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture;
}

describe('MostSearchedProductsComponent', () => {
  beforeEach(() => {
    dashboard.getMostSearchedProducts.mockReset();
    dashboard.incrementProductViewCount.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
    dashboard.getMostSearchedProducts.mockReturnValue(of({
      success: true,
      message: null,
      data: [product(), product({ productId: '22222222-2222-2222-2222-222222222222', productName: 'HG Zaku II' })]
    }));
  });

  it('calls the block most viewed because the API orders by ProductViewCount', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;
    const heading = host.querySelector('h4');
    const text = host.textContent ?? '';

    expect(heading?.textContent?.trim()).toBe('Productos Más Vistos');
    expect(text).not.toContain('Productos Más Buscados');
    expect(text).not.toContain('Buscados');
    expect(text).not.toContain('búsquedas');
  });

  it('labels the view counter as vistas because no search log exists', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;
    const counter = host.querySelector('.card-body small');

    expect(counter?.textContent).toContain('vistas');
    expect(counter?.textContent).not.toContain('búsquedas');
    expect(counter?.textContent).toContain('1,234');
  });

  it('renders no trending or popular badge for any card', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;
    const text = host.textContent ?? '';

    expect(host.querySelectorAll('.card').length).toBe(2);
    expect(text).not.toContain('TRENDING');
    expect(text).not.toContain('POPULAR');
    expect(host.querySelector('.badge-trending')).toBeNull();
    expect(host.querySelector('.badge-popular')).toBeNull();
  });

  it('uses the real placeholder asset path without a query string', async () => {
    const fixture = await render();
    const image = (fixture.nativeElement as HTMLElement).querySelector('img');

    expect(image?.getAttribute('src')).toBe('assets/images/placeholder.svg');
    expect(image?.getAttribute('alt')).toBe('RX-78-2 Gundam');
  });
});
