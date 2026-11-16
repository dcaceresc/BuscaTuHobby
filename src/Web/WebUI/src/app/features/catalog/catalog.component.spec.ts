import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { of, Subject, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { CatalogComponent } from './catalog.component';
import { routes } from '../../app.routes';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';
import { CatalogProductDto, CategoryProductsDto } from '@app/core/models/dashboard/dashboard.model';

const dashboard = { getCategoryProducts: vi.fn() };
const notifications = { showError: vi.fn(), showDefaultError: vi.fn() };

function catalogProduct(overrides: Partial<CatalogProductDto> = {}): CatalogProductDto {
  return {
    productId: '11111111-1111-1111-1111-111111111111',
    productName: 'RX-78-2 Gundam',
    manufacturerName: 'Bandai',
    franchiseName: 'Gundam',
    grade: 2,
    scale: '1/100',
    partNumber: 'MG-001',
    minPrice: 19990,
    maxPrice: 24990,
    storeCount: 2,
    imageUrls: ['/images/abc.jpg'],
    ...overrides
  };
}

function categoryPage(overrides: Partial<CategoryProductsDto> = {}): CategoryProductsDto {
  return {
    categoryName: 'Gunpla',
    categorySlug: 'gunpla',
    total: 2,
    page: 1,
    pageSize: 12,
    items: [
      catalogProduct(),
      catalogProduct({
        productId: '22222222-2222-2222-2222-222222222222',
        productName: 'Zaku II',
        grade: 0,
        scale: null,
        partNumber: null,
        minPrice: 8990,
        maxPrice: 8990,
        storeCount: 1,
        imageUrls: []
      })
    ],
    manufacturers: [{ value: 'm1', label: 'Bandai', count: 2 }],
    franchises: [{ value: 'f1', label: 'Gundam', count: 2 }],
    grades: [{ value: '2', label: 'Master Grade (MG)', count: 1 }],
    ...overrides
  };
}

function successPage(overrides: Partial<CategoryProductsDto> = {}) {
  return of({ success: true, message: '', data: categoryPage(overrides) });
}

async function render(url = '/categories/gunpla'): Promise<RouterTestingHarness> {
  TestBed.configureTestingModule({
    providers: [
      provideRouter([{ path: 'categories/:slug', component: CatalogComponent }]),
      { provide: DashboardService, useValue: dashboard },
      { provide: NotificationService, useValue: notifications }
    ]
  });
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl(url, CatalogComponent);
  await flush(harness);
  return harness;
}

/** Deja correr la navegación y la recarga que dispara el cambio de la URL. */
async function flush(harness: RouterTestingHarness): Promise<void> {
  await harness.fixture.whenStable();
  harness.fixture.detectChanges();
  await harness.fixture.whenStable();
  harness.fixture.detectChanges();
}

function change(select: HTMLSelectElement, value: string): void {
  select.value = value;
  select.dispatchEvent(new Event('change'));
}

describe('CatalogComponent', () => {
  beforeEach(() => {
    dashboard.getCategoryProducts.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
    dashboard.getCategoryProducts.mockReturnValue(successPage());
  });

  it('is declared as a lazy route inside the layout, before the wildcard', async () => {
    const children = routes.find(route => route.path === '')?.children ?? [];
    const index = children.findIndex(child => child.path === 'categories/:slug');
    const wildcard = children.findIndex(child => child.path === '**');

    expect(index).toBeGreaterThan(-1);
    expect(index).toBeLessThan(wildcard);
    const loaded = await children[index].loadComponent!();
    expect(loaded).toBe(CatalogComponent);
  });

  it('renders the category heading with its total and one card per product', async () => {
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;
    const cards = Array.from(host.querySelectorAll<HTMLElement>('.catalog__card'));

    expect(host.querySelector('h1')?.textContent).toContain('Gunpla');
    expect(host.textContent).toContain('2 productos');
    expect(cards).toHaveLength(2);
    expect(dashboard.getCategoryProducts).toHaveBeenCalledWith('gunpla', expect.objectContaining({
      page: 1,
      pageSize: 12,
      sort: 'name'
    }));
  });

  it('shows the price range, the store count and the catalog fields of each card', async () => {
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;
    const first = host.querySelector<HTMLElement>('.catalog__card') as HTMLElement;
    const second = host.querySelectorAll<HTMLElement>('.catalog__card')[1];

    expect(first.textContent).toContain('RX-78-2 Gundam');
    expect(first.textContent).toContain('Bandai');
    expect(first.textContent).toContain('Gundam');
    expect(first.textContent).toContain('Master Grade (MG)');
    expect(first.textContent).toContain('1/100');
    expect(first.textContent).toContain('MG-001');
    expect(first.textContent).toContain('$19.990');
    expect(first.textContent).toContain('$24.990');
    expect(first.textContent).toContain('2 tiendas');
    expect(first.querySelector('a')?.getAttribute('href'))
      .toBe('/products/11111111-1111-1111-1111-111111111111');
    expect(first.querySelector('img')?.getAttribute('src')).toBe('/images/abc.jpg');

    // Un producto sin grado, escala ni número de parte omite esos datos, y sin imágenes usa el placeholder.
    expect(second.textContent).not.toContain('Master Grade (MG)');
    expect(second.querySelector('img')?.getAttribute('src')).toBe('assets/images/placeholder.svg');
    expect(second.textContent).toContain('1 tienda');
  });

  it('lists the facets with their counts', async () => {
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;

    expect(host.querySelector('#manufacturerId option[value="m1"]')?.textContent).toContain('Bandai (2)');
    expect(host.querySelector('#franchiseId option[value="f1"]')?.textContent).toContain('Gundam (2)');
    expect(host.querySelector('#grade option[value="2"]')?.textContent).toContain('Master Grade (MG) (1)');
  });

  it('sends a chosen filter as a query parameter and reloads from it', async () => {
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;

    change(host.querySelector<HTMLSelectElement>('#manufacturerId') as HTMLSelectElement, 'm1');
    await flush(harness);

    expect(TestBed.inject(Router).url).toContain('manufacturerId=m1');
    expect(dashboard.getCategoryProducts).toHaveBeenLastCalledWith('gunpla', expect.objectContaining({
      page: 1,
      manufacturerId: 'm1'
    }));
  });

  it('sends the chosen sort and reads it back from the URL', async () => {
    const harness = await render('/categories/gunpla?sort=price-desc&page=2');
    const host = harness.routeNativeElement as HTMLElement;

    expect(dashboard.getCategoryProducts).toHaveBeenCalledWith('gunpla', expect.objectContaining({
      sort: 'price-desc',
      page: 2
    }));

    change(host.querySelector<HTMLSelectElement>('#sort') as HTMLSelectElement, 'views');
    await flush(harness);

    expect(TestBed.inject(Router).url).toContain('sort=views');
    expect(dashboard.getCategoryProducts).toHaveBeenLastCalledWith('gunpla', expect.objectContaining({
      sort: 'views',
      page: 1
    }));
  });

  it('paginates from the URL and keeps the page there', async () => {
    dashboard.getCategoryProducts.mockReturnValue(successPage({ total: 30 }));
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;
    const pages = Array.from(host.querySelectorAll<HTMLButtonElement>('.pagination .page-link'));

    const secondPage = pages.find(button => button.textContent?.trim() === '2') as HTMLButtonElement;
    expect(secondPage).toBeDefined();

    secondPage.click();
    await flush(harness);

    expect(TestBed.inject(Router).url).toContain('page=2');
    expect(dashboard.getCategoryProducts).toHaveBeenLastCalledWith('gunpla', expect.objectContaining({ page: 2 }));
  });

  it('shows a Spanish message when no product matches', async () => {
    dashboard.getCategoryProducts.mockReturnValue(successPage({ total: 0, items: [] }));
    const harness = await render();
    const text = harness.routeNativeElement?.textContent ?? '';

    expect(text).toContain('No hay productos en esta categoría con los filtros seleccionados.');
    expect(harness.routeNativeElement?.querySelectorAll('.catalog__card').length).toBe(0);
  });

  it('says "Sin ofertas" instead of inventing a price when the product has no eligible offer', async () => {
    dashboard.getCategoryProducts.mockReturnValue(successPage({
      total: 2,
      items: [
        catalogProduct({ minPrice: null, maxPrice: null, storeCount: 0 }),
        catalogProduct({
          productId: '33333333-3333-3333-3333-333333333333',
          productName: 'Zaku II',
          minPrice: 0,
          maxPrice: 0,
          storeCount: 1
        })
      ]
    }));
    const harness = await render();
    const host = harness.routeNativeElement as HTMLElement;
    const cards = Array.from(host.querySelectorAll<HTMLElement>('.catalog__card'));

    // Sin ofertas elegibles no hay precio ni conteo de tiendas: la ficha no inventa un cero.
    expect(cards[0].textContent).toContain('Sin ofertas');
    expect(cards[0].textContent).not.toContain('$');
    expect(cards[0].textContent).not.toContain('tienda');
    expect(cards[0].querySelector('.catalog__price')).toBeNull();

    // Un precio realmente cero se sigue mostrando como cero.
    expect(cards[1].textContent).toContain('$0');
    expect(cards[1].textContent).toContain('1 tienda');
    expect(cards[1].textContent).not.toContain('Sin ofertas');
  });

  it('requests once per navigation and never lets a stale category overwrite the current one', async () => {
    const uno = new Subject<any>();
    const dos = new Subject<any>();
    dashboard.getCategoryProducts.mockImplementation((slug: string) => slug === 'uno' ? uno : dos);

    const harness = await render('/categories/uno?grade=1&page=2');
    const before = dashboard.getCategoryProducts.mock.calls.length;

    expect(before).toBe(1);
    expect(dashboard.getCategoryProducts).toHaveBeenCalledWith('uno', expect.objectContaining({
      grade: '1',
      page: 2
    }));

    await harness.navigateByUrl('/categories/dos');
    await flush(harness);

    // Una sola petición para la navegación, con el argumento nuevo y sin los filtros de la categoría anterior.
    const issued = dashboard.getCategoryProducts.mock.calls.slice(before);
    expect(issued).toHaveLength(1);
    expect(issued[0][0]).toBe('dos');
    expect(issued[0][1]).toMatchObject({ page: 1, grade: '', sort: 'name' });

    // La respuesta de la categoría abandonada llega tarde y no puede reemplazar la vigente.
    dos.next({ success: true, message: '', data: categoryPage({ categoryName: 'Dos', categorySlug: 'dos' }) });
    dos.complete();
    uno.next({ success: true, message: '', data: categoryPage({ categoryName: 'Uno', categorySlug: 'uno' }) });
    uno.complete();
    await flush(harness);

    expect((harness.routeNativeElement as HTMLElement).querySelector('h1')?.textContent).toContain('Dos');
  });

  it('says the requested page does not exist and offers the way back to the first page', async () => {
    dashboard.getCategoryProducts.mockReturnValue(successPage({ total: 30, page: 99, items: [] }));
    const harness = await render('/categories/gunpla?page=99');
    const host = harness.routeNativeElement as HTMLElement;

    expect(host.textContent).toContain('La página 99 no existe');
    expect(host.textContent).toContain('30 productos');
    expect(host.textContent).not.toContain('con los filtros seleccionados');
    // El paginador sigue visible para que la vista no quede sin salida.
    expect(host.querySelectorAll('.pagination .page-link').length).toBeGreaterThan(1);

    (host.querySelector('.catalog__first-page') as HTMLButtonElement).click();
    await flush(harness);

    expect(TestBed.inject(Router).url).not.toContain('page=99');
    expect(dashboard.getCategoryProducts).toHaveBeenLastCalledWith('gunpla', expect.objectContaining({ page: 1 }));
  });

  it('reports a failed envelope instead of rendering a blank page', async () => {
    dashboard.getCategoryProducts.mockReturnValue(of({
      success: false, message: 'La categoría no existe.', data: null
    }));
    const harness = await render();

    expect(harness.routeNativeElement?.textContent).toContain('La categoría no existe.');
    expect(notifications.showError).toHaveBeenCalledWith('Error', 'La categoría no existe.');
  });

  it('reports a failed request instead of rendering a blank page', async () => {
    dashboard.getCategoryProducts.mockReturnValue(throwError(() => new Error('boom')));
    const harness = await render();

    expect(harness.routeNativeElement?.textContent).toContain('No pudimos cargar los productos de esta categoría.');
    expect(notifications.showDefaultError).toHaveBeenCalled();
  });
});
