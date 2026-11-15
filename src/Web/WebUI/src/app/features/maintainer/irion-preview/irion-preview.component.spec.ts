import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Subject } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { IrionPreviewComponent } from './irion-preview.component';
import { IrionProductPreviewService, NotificationService } from '@app/core/services';
import { ApiResponse, IrionProductPreviewDto } from '@app/core/models';
import { routes } from '../maintainer.routes';
import { AdministrationMenuComponent } from '@app/shared/layouts/main-layout/components/header/administration-menu/administration-menu.component';
import { AuthService } from '@app/core/services';
import { NgbActiveOffcanvas } from '@ng-bootstrap/ng-bootstrap/offcanvas';

const url = 'https://irion.cl/products/optimus-prime';
const product: IrionProductPreviewDto = {
  sourceUrl: url, title: 'Optimus Prime', vendor: 'Blokees', currency: 'CLP',
  descriptionHtml: '<img src=x onerror="alert(1)">Figura de 13 cm',
  tags: ['TRANSFORMERS'], imageUrls: ['https://cdn.shopify.com/figure.png'],
  available: true,
  variants: [{ id: '123', sku: null, barcode: null, available: true,
    price: 21990, originalPrice: null, discountPercentage: null }]
};

describe('IrionPreviewComponent', () => {
  let response: Subject<ApiResponse<IrionProductPreviewDto>>;
  const preview = vi.fn();
  const showError = vi.fn();

  beforeEach(async () => {
    response = new Subject();
    preview.mockReset().mockReturnValue(response);
    showError.mockReset();
    await TestBed.configureTestingModule({
      imports: [IrionPreviewComponent],
      providers: [
        { provide: IrionProductPreviewService, useValue: { preview } },
        { provide: NotificationService, useValue: { showError } }
      ]
    }).compileComponents();
  });

  async function setup() {
    const fixture = TestBed.createComponent(IrionPreviewComponent);
    await fixture.whenStable();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    const submit = async (value: string) => {
      input.value = value;
      input.dispatchEvent(new Event('input'));
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();
    };
    return { fixture, submit };
  }

  it('starts empty and rejects empty or non-Irion product URLs without a request', async () => {
    const { fixture, submit } = await setup();
    expect(fixture.nativeElement.textContent).toContain('sin guardar');
    for (const value of ['', 'https://evil.test/products/a', 'https://irion.cl/admin',
      'https://evil.test/collections/all/products/a', 'https://user@irion.cl/collections/all/products/a',
      'https://irion.cl:444/collections/all/products/a', 'https://irion.cl/collections/../products/a',
      'https://irion.cl/collections/all/../products/a', 'https://irion.cl/collections/all/products/a%2fb',
      'https://irion.cl/collections/all/products/a%5cb', 'https://irion.cl/collections/all/products/a\\b',
      'https://irion.cl/collections/all/products/a/b', 'https://irion.cl/collections/all/products/a.js',
      'https://irion.cl/collections/-all/products/a', 'https://irion.cl/collections/all/products/a--b']) {
      await submit(value);
      expect(preview).not.toHaveBeenCalled();
      expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
    }
  });

  it('loads once, shows the preview and never interprets source HTML', async () => {
    const { fixture, submit } = await setup();
    await submit(url);
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(true);
    await submit(url);
    expect(preview).toHaveBeenCalledExactlyOnceWith(url);
    response.next({ success: true, message: '', data: product });
    response.complete();
    await fixture.whenStable();
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Optimus Prime');
    expect(text).toContain('Blokees');
    expect(text).toContain('21.990');
    expect(text).toContain('Sin información');
    expect(text).toContain('Sin descuento informado');
    expect(fixture.nativeElement.querySelectorAll('img').length).toBe(1);
    expect(fixture.nativeElement.querySelector('[onerror]')).toBeNull();
    expect(fixture.nativeElement.querySelector('pre').textContent).toBe(product.descriptionHtml);
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(false);
    expect(fixture.nativeElement.querySelector('a').getAttribute('href')).toBe(url);
  });

  it('clears stale data and permits retry after envelope and transport failures', async () => {
    const { fixture, submit } = await setup();
    await submit(url);
    response.next({ success: true, message: '', data: product });
    response.complete();
    await fixture.whenStable();
    for (const transportFailure of [false, true]) {
      response = new Subject();
      preview.mockReturnValue(response);
      await submit(url);
      expect(fixture.nativeElement.querySelector('h2')).toBeNull();
      if (transportFailure) response.error(new Error('offline'));
      else {
        response.next({ success: false, message: 'No se pudo extraer.', data: null! });
        response.complete();
      }
      await fixture.whenStable();
      expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
      expect(fixture.nativeElement.querySelector('button').disabled).toBe(false);
      expect((fixture.nativeElement.querySelector('input') as HTMLInputElement).value).toBe(url);
      expect(showError).toHaveBeenCalled();
    }
    response = new Subject();
    preview.mockReturnValue(response);
    await submit(url);
    response.next({ success: true, message: '', data: { ...product, available: false,
      imageUrls: [], tags: [], variants: [{ ...product.variants[0], available: false,
        originalPrice: 24990, discountPercentage: 12 }] } });
    response.complete();
    await fixture.whenStable();
    expect(fixture.nativeElement.textContent).toContain('Sin disponibilidad');
    expect(fixture.nativeElement.textContent).toContain('12%');
    expect(fixture.nativeElement.textContent).toContain('Sin imágenes');
  });

  it.each([
    ['https://irion.cl/products/bandai-v-gundam-ver-ka-mg-1100?_pos=1&_sid=1e1ac7564&_ss=r',
      'Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.'],
    ['https://irion.cl/collections/productos-mas-recientes/products/transformers-action-edition-07-optimus-prime-ver-2-0',
      'Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.'],
    [url, 'Irion está limitando las consultas (HTTP 429). Espera al menos 30 segundos antes de reintentar manualmente.'],
    [url, 'Irion está limitando las consultas (HTTP 429). No vuelvas a consultar antes de 2099-01-01 00:00:00 UTC; reintenta manualmente.']
  ])('accepts the supplied URL and displays a 429 envelope without retry: %s', async (value, message) => {
    const { fixture, submit } = await setup();
    await submit(value);
    expect(preview).toHaveBeenCalledExactlyOnceWith(value);
    response.next({ success: false, message, data: null! });
    response.complete();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toBe(message);
    expect(showError).toHaveBeenCalledExactlyOnceWith('Vista previa de Irion', message);
    expect(fixture.nativeElement.querySelector('input').value).toBe(value);
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(false);
    expect(fixture.nativeElement.querySelector('h2')).toBeNull();
    expect(preview).toHaveBeenCalledTimes(1);
  });

  it('adds a lazy maintainer route without removing existing product navigation', () => {
    expect(routes.find(route => route.path === 'irion-preview')?.loadComponent).toBeDefined();
    expect(routes.find(route => route.path === 'products')?.loadChildren).toBeDefined();
  });
});

describe('Irion maintainer navigation', () => {
  it('renders the Irion option alongside the existing Products option', async () => {
    await TestBed.configureTestingModule({
      imports: [AdministrationMenuComponent],
      providers: [provideRouter([]),
        { provide: AuthService, useValue: { isSuperAdmin: () => false } },
        { provide: NgbActiveOffcanvas, useValue: { dismiss: vi.fn() } }]
    }).compileComponents();
    const fixture = TestBed.createComponent(AdministrationMenuComponent);
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('a[href="/maintainer/irion-preview"]').textContent)
      .toContain('Vista previa de Irion');
    expect(fixture.nativeElement.querySelector('a[href="/maintainer/products"]')).not.toBeNull();
  });
});

describe('IrionProductPreviewService', () => {
  it('posts only the URL and returns the API envelope', () => {
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] });
    const http = TestBed.inject(HttpTestingController);
    const service = TestBed.inject(IrionProductPreviewService);
    let received: ApiResponse<IrionProductPreviewDto> | undefined;
    service.preview(url).subscribe(value => received = value);
    const request = http.expectOne('/api/maintainer/products/preview/irion');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ url });
    request.flush({ success: true, message: '', data: product });
    expect(received?.data).toEqual(product);
    http.verify();
  });
});
