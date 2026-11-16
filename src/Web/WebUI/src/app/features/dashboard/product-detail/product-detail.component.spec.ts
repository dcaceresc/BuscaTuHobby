import { registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ProductDetailComponent } from './product-detail.component';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';
import { ProductDetailDto } from '@app/core/models/dashboard/dashboard.model';

registerLocaleData(localeEsCl, 'es-CL');

function detail(overrides: Partial<ProductDetailDto> = {}): ProductDetailDto {
  return {
    productId: '11111111-1111-1111-1111-111111111111',
    productName: 'RX-78-2 Gundam',
    productDescription: 'Maqueta a escala',
    manufacturerName: 'Bandai',
    franchiseName: 'Gundam',
    serieName: null,
    productHasBase: true,
    productTargetAge: '8+',
    productSize: '18cm',
    productReleaseDate: '2024-01-15',
    productViewCount: 1234,
    categories: ['Maquetas'],
    imageUrls: [],
    storeCount: 2,
    minPrice: 19990,
    maxPrice: 25000,
    offers: [
      {
        inventoryId: 'a', storeId: 's1', storeName: 'Tienda Barata', storeSlug: 'barata',
        storeIcon: 'bi bi-shop', storeWebSite: 'https://barata.cl', price: 19990, originalPrice: 24990,
        discountPercentage: 20, availability: true, sourceUrl: 'https://barata.cl/p/1', observedAtUtc: null
      },
      {
        inventoryId: 'b', storeId: 's2', storeName: 'Tienda Cara', storeSlug: 'cara',
        storeIcon: 'bi bi-shop', storeWebSite: 'https://cara.cl', price: 25000, originalPrice: 25000,
        discountPercentage: 0, availability: false, sourceUrl: null, observedAtUtc: null
      }
    ],
    ...overrides
  };
}

const dashboard = {
  getProductDetail: vi.fn(),
  getProductPriceHistory: vi.fn(),
  incrementProductViewCount: vi.fn()
};
const notifications = { showError: vi.fn(), showDefaultError: vi.fn() };

async function render(productId = '11111111-1111-1111-1111-111111111111') {
  TestBed.configureTestingModule({
    imports: [ProductDetailComponent],
    providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ productId }) } } },
      { provide: DashboardService, useValue: dashboard },
      { provide: NotificationService, useValue: notifications }
    ]
  });
  const fixture = TestBed.createComponent(ProductDetailComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture;
}

describe('ProductDetailComponent', () => {
  beforeEach(() => {
    dashboard.getProductDetail.mockReset();
    dashboard.getProductPriceHistory.mockReset();
    dashboard.incrementProductViewCount.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
    dashboard.getProductDetail.mockReturnValue(of({ success: true, message: null, data: detail() }));
    dashboard.getProductPriceHistory.mockReturnValue(of({
      success: true, message: null,
      data: {
        productId: '11111111-1111-1111-1111-111111111111', productName: 'RX-78-2 Gundam', hasHistory: false, points: []
      }
    }));
    dashboard.incrementProductViewCount.mockReturnValue(of({ success: true, message: null }));
  });

  it('renders the product characteristics returned by the API', async () => {
    const fixture = await render();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('RX-78-2 Gundam');
    expect(text).toContain('Maqueta a escala');
    expect(text).toContain('Bandai');
    expect(text).toContain('Gundam');
    expect(text).toContain('18cm');
    expect(text).toContain('8+');
    expect(fixture.nativeElement.querySelectorAll('dd')[1].textContent).toContain('8+');
  });

  it('lists the stores in the order the API returned them', async () => {
    const fixture = await render();
    const rows = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr'));

    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('Tienda Barata');
    expect(rows[0].textContent).toContain('19.990');
    expect(rows[0].textContent).toContain('Más barata');
    expect(rows[1].textContent).toContain('Tienda Cara');
    expect(rows[1].textContent).not.toContain('Más barata');
  });

  it('flags an unavailable offer and links the ones with a source URL', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;

    expect(host.querySelector('tbody tr:nth-child(2)')?.textContent).toContain('Sin stock');
    expect(host.querySelector('tbody tr:nth-child(1) a')?.getAttribute('href')).toBe('https://barata.cl/p/1');
    expect(host.querySelector('tbody tr:nth-child(2) a')?.getAttribute('href')).toBe('https://cara.cl');
  });

  it('falls back to the placeholder image when the product has none', async () => {
    const fixture = await render();
    const image = (fixture.nativeElement as HTMLElement).querySelector('img.product-detail__image');

    expect(image?.getAttribute('src')).toBe('assets/images/placeholder.svg');
  });

  it('shows the price chart empty state when there is no recorded history', async () => {
    const fixture = await render();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('Todavía no hay precios registrados para este producto.');
    expect(fixture.nativeElement.querySelector('app-price-chart svg')).toBeNull();
  });

  it('reports a missing product instead of rendering an empty page', async () => {
    dashboard.getProductDetail.mockReturnValue(of({
      success: false, message: 'El producto no existe o no está disponible.', data: null
    }));
    const fixture = await render();

    expect((fixture.nativeElement as HTMLElement).textContent)
      .toContain('No encontramos este producto o ya no está disponible.');
    expect(notifications.showError).toHaveBeenCalledWith('Error', 'El producto no existe o no está disponible.');
    expect(dashboard.incrementProductViewCount).not.toHaveBeenCalled();
  });

  it('counts one view when the product loads', async () => {
    await render();

    expect(dashboard.incrementProductViewCount).toHaveBeenCalledExactlyOnceWith('11111111-1111-1111-1111-111111111111');
  });

  it('keeps the page usable when the series request fails', async () => {
    dashboard.getProductPriceHistory.mockReturnValue(throwError(() => new Error('boom')));
    const fixture = await render();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('RX-78-2 Gundam');
    expect(notifications.showDefaultError).not.toHaveBeenCalled();
  });
});
