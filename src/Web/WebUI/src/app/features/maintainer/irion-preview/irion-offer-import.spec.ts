import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of, Subject } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { IrionPreviewComponent } from './irion-preview.component';
import { IrionOfferImportService, IrionProductPreviewService, NotificationService, ProductService, StoreService } from '@app/core/services';

const sourceUrl = 'https://irion.cl/products/model-1';
const variant = { id: '456', sku: 'Elegida', barcode: null, price: 200, originalPrice: 300, available: false, discountPercentage: 33 };
const preview = { sourceUrl, title: 'Modelo', vendor: '', currency: 'CLP', descriptionHtml: '<img onerror="alert(1)">', tags: [], imageUrls: [], available: true,
  variants: [{ ...variant, id: '123', price: 100 }, variant] };
const selection = { productId: 'p1', storeId: 's1', sourceUrl, externalVariantId: '456' };
const inspection = { ...selection, outcome: 'Create', inventoryId: null, rowVersion: null, isActive: null, message: null };
const envelope = <T>(data: T, success = true, message = '') => ({ success, message, data });

describe('Importación explícita de Irion', () => {
  const inspect = vi.fn();
  const confirm = vi.fn();
  const getProducts = vi.fn();
  const getStores = vi.fn();
  const showSuccess = vi.fn();
  let inspected: Subject<any>;
  let confirmed: Subject<any>;

  beforeEach(async () => {
    inspected = new Subject();
    confirmed = new Subject();
    inspect.mockReset().mockReturnValue(inspected);
    confirm.mockReset().mockReturnValue(confirmed);
    showSuccess.mockReset();
    getProducts.mockReset().mockReturnValue(of(envelope([
      { productId: 'p1', productName: 'Activo', isActive: true },
      { productId: 'p2', productName: 'Otro activo', isActive: true },
      { productId: 'inactive', productName: 'Inactivo', isActive: false }
    ])));
    getStores.mockReset().mockReturnValue(of(envelope([
      { storeId: 's1', storeName: 'Irion', storeWebSite: 'https://IRION.cl:443/', isActive: true },
      ...['https://irion.cl.evil.test', 'https://irion.cl/products/a', 'http://irion.cl', 'https://user@irion.cl', 'https://irion.cl/?x=1', 'https://irion.cl:444', ' https://irion.cl', 'https://irion.cl\n', 'https://irion.cl/ ' ].map((storeWebSite, i) => ({ storeId: `bad${i}`, storeName: 'No válida', storeWebSite, isActive: true })),
      { storeId: 'disabled', storeName: 'Inactiva', storeWebSite: 'https://irion.cl', isActive: false }
    ])));
    await TestBed.configureTestingModule({ imports: [IrionPreviewComponent], providers: [
      { provide: IrionProductPreviewService, useValue: { preview: vi.fn(() => of(envelope(preview))) } },
      { provide: IrionOfferImportService, useValue: { inspect, confirm } },
      { provide: ProductService, useValue: { getProducts } },
      { provide: StoreService, useValue: { getStores } },
      { provide: NotificationService, useValue: { showSuccess, showError: vi.fn() } }
    ] }).compileComponents();
  });

  async function setup() {
    const fixture = TestBed.createComponent(IrionPreviewComponent);
    await fixture.whenStable();
    const dom = fixture.nativeElement as HTMLElement;
    const change = async (id: string, value: string) => {
      const input = dom.querySelector(`#${id}`) as HTMLInputElement;
      input.value = value;
      input.dispatchEvent(new Event(input.tagName === 'SELECT' ? 'change' : 'input'));
      await fixture.whenStable();
    };
    const click = async (text: string) => {
      const button = [...dom.querySelectorAll('button')].find(b => b.textContent?.includes(text))!;
      button.click();
      await fixture.whenStable();
      return button;
    };
    await change('irion-url', sourceUrl);
    await click('Visualizar producto');
    const choose = async () => {
      await change('irion-product', 'p1');
      await change('irion-store', 's1');
      await change('irion-variant', '456');
    };
    const review = async (data: { [key: string]: unknown } = inspection) => {
      await choose();
      await click('Revisar vinculación');
      inspected.next(envelope(data));
      inspected.complete();
      await fixture.whenStable();
    };
    return { fixture, dom, change, click, choose, review };
  }

  it('filters active existing options by exact Irion root and requires an explicit variant, even when unique', async () => {
    const { dom, change, click } = await setup();
    expect([...dom.querySelectorAll('#irion-product option')].map(o => o.getAttribute('value'))).toEqual(['', 'p1', 'p2']);
    expect([...dom.querySelectorAll('#irion-store option')].map(o => o.getAttribute('value'))).toEqual(['', 's1']);
    expect((dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('');
    await change('irion-product', 'p1');
    await change('irion-store', 's1');
    expect((await click('Revisar vinculación')).disabled).toBe(true);
    expect(inspect).not.toHaveBeenCalled();
    expect(confirm).not.toHaveBeenCalled();
    expect(dom.querySelector('[onerror]')).toBeNull();
    expect(dom.querySelector('pre')?.textContent).toBe(preview.descriptionHtml);
  });

  it.each(['Create', 'Update', 'Conflict'])('shows %s summary and only explicitly confirms a non-conflicting inspected snapshot', async outcome => {
    const ui = await setup();
    const data = { ...inspection, outcome, inventoryId: outcome === 'Create' ? null : 'inventory', rowVersion: outcome === 'Create' ? null : 'AQIDBAUGBwg=', isActive: false };
    await ui.review(data);
    expect(inspect).toHaveBeenCalledExactlyOnceWith(selection);
    expect(confirm).not.toHaveBeenCalled();
    expect(ui.dom.textContent).toContain(outcome === 'Create' ? 'Se creará' : outcome === 'Update' ? 'Se actualizará' : 'Conflicto');
    if (outcome === 'Update') expect(ui.dom.textContent).toContain('permanecerá deshabilitado');
    await ui.click('Guardar oferta de Irion');
    if (outcome === 'Conflict') expect(confirm).not.toHaveBeenCalled();
    else {
      await ui.click('Guardar oferta de Irion');
      expect(confirm).toHaveBeenCalledExactlyOnceWith({ ...selection, expectedInventoryId: data.inventoryId, expectedRowVersion: data.rowVersion,
        expectedOffer: { currency: 'CLP', price: 200, originalPrice: 300, available: false } });
      expect((ui.dom.querySelector('#irion-url') as HTMLInputElement).readOnly).toBe(true);
      expect(ui.dom.querySelector('#irion-product')?.matches(':disabled')).toBe(true);
      const previewService = TestBed.inject(IrionProductPreviewService);
      await ui.click('Visualizar producto');
      await ui.click('Cancelar');
      expect(previewService.preview).toHaveBeenCalledTimes(1);
      expect(confirm).toHaveBeenCalledTimes(1);
    }
  });

  it.each(['Saved', 'AlreadySaved'])('announces %s once and disarms without automatic resubmission', async outcome => {
    const ui = await setup();
    await ui.review();
    await ui.click('Guardar oferta de Irion');
    confirmed.next(envelope({ outcome, inventoryId: 'inventory' }));
    confirmed.complete();
    await ui.fixture.whenStable();
    expect(showSuccess).toHaveBeenCalledTimes(1);
    expect(ui.dom.textContent).toContain(outcome === 'Saved' ? 'Oferta guardada' : 'La oferta ya estaba guardada');
    expect((await ui.click('Guardar oferta de Irion')).disabled).toBe(true);
    expect(confirm).toHaveBeenCalledTimes(1);
  });

  it.each([['irion-product', 'p2'], ['irion-store', ''], ['irion-variant', '123'], ['irion-url', 'https://irion.cl/products/other']])('invalidates an armed review when %s changes without saving', async (id, value) => {
    const ui = await setup();
    await ui.review();
    await ui.change(id, value);
    expect((await ui.click('Guardar oferta de Irion')).disabled).toBe(true);
    expect(confirm).not.toHaveBeenCalled();
  });

  it.each(['Cancelar', 'Visualizar producto'])('%s clears an armed review without saving', async action => {
    const ui = await setup();
    await ui.review();
    await ui.click(action);
    expect((ui.dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('');
    expect((await ui.click('Guardar oferta de Irion')).disabled).toBe(true);
    expect(confirm).not.toHaveBeenCalled();
  });

  it('does not select the only variant automatically', async () => {
    vi.mocked(TestBed.inject(IrionProductPreviewService).preview).mockReturnValue(of(envelope({ ...preview, variants: [variant] })));
    const ui = await setup();
    await ui.change('irion-product', 'p1');
    await ui.change('irion-store', 's1');
    expect((ui.dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('');
    expect((await ui.click('Revisar vinculación')).disabled).toBe(true);
    expect(inspect).not.toHaveBeenCalled();
    expect(confirm).not.toHaveBeenCalled();
  });

  it.each(['envelope', 'transport', 'mismatched'])('does not arm saving after an inspection %s failure', async failure => {
    const ui = await setup();
    await ui.choose();
    await ui.click('Revisar vinculación');
    if (failure === 'transport') inspected.error(new Error('offline'));
    else {
      inspected.next(failure === 'mismatched' ? envelope({ ...inspection, externalVariantId: '123' }) : envelope(null, false));
      inspected.complete();
    }
    await ui.fixture.whenStable();
    expect(ui.dom.querySelector('[role="alert"]')).not.toBeNull();
    expect((await ui.click('Guardar oferta de Irion')).disabled).toBe(true);
    expect((ui.dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('456');
    expect(inspect).toHaveBeenCalledTimes(1);
    expect(confirm).not.toHaveBeenCalled();
  });

  it('blocks overlapping inspection and rejects a stale response after programmatic selection change', async () => {
    const ui = await setup();
    await ui.choose();
    await ui.click('Revisar vinculación');
    await ui.click('Revisar vinculación');
    expect(inspect).toHaveBeenCalledTimes(1);
    expect(ui.dom.querySelector('#irion-variant')?.matches(':disabled')).toBe(true);
    // A stale response must remain harmless even if the form changes outside disabled human controls.
    ui.fixture.componentInstance.offerForm.controls.productId.setValue('p2');
    inspected.next(envelope(inspection));
    inspected.complete();
    await ui.fixture.whenStable();
    expect((await ui.click('Guardar oferta de Irion')).disabled).toBe(true);
    expect(confirm).not.toHaveBeenCalled();
  });

  it.each(['429', 'transport', 'Conflict'])('recovers from %s without retry or lost URL/selections and requires new review', async failure => {
    const ui = await setup();
    await ui.review();
    await ui.click('Guardar oferta de Irion');
    if (failure === 'transport') confirmed.error(new Error('timeout'));
    else {
      confirmed.next(failure === 'Conflict' ? envelope({ outcome: 'Conflict' }, false) : envelope(null, false, 'Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.'));
      confirmed.complete();
    }
    await ui.fixture.whenStable();
    expect(ui.dom.querySelector('[role="alert"]')).not.toBeNull();
    expect((ui.dom.querySelector('#irion-url') as HTMLInputElement).value).toBe(sourceUrl);
    expect((ui.dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('456');
    expect((await ui.click('Guardar oferta de Irion')).disabled).toBe(true);
    expect(confirm).toHaveBeenCalledTimes(1);
  });

  it('uses fresh preview source and clears variant on RefreshRequired, requiring manual review and another save', async () => {
    const ui = await setup();
    await ui.review();
    await ui.click('Guardar oferta de Irion');
    const freshSource = 'https://irion.cl/products/fresh';
    confirmed.next(envelope({ outcome: 'RefreshRequired', freshPreview: { ...preview, sourceUrl: freshSource, title: 'Actualizado', variants: [{ ...variant, price: 250 }] }, inspection }, false));
    confirmed.complete();
    await ui.fixture.whenStable();
    expect(ui.dom.textContent).toContain('Actualizado');
    expect((ui.dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('');
    expect((await ui.click('Guardar oferta de Irion')).disabled).toBe(true);
    expect(confirm).toHaveBeenCalledTimes(1);
    inspected = new Subject();
    inspect.mockReturnValue(inspected);
    await ui.change('irion-variant', '456');
    await ui.click('Revisar vinculación');
    expect(inspect).toHaveBeenLastCalledWith({ ...selection, sourceUrl: freshSource });
    inspected.next(envelope({ ...inspection, sourceUrl: freshSource }));
    inspected.complete();
    await ui.fixture.whenStable();
    expect(confirm).toHaveBeenCalledTimes(1);
    confirmed = new Subject();
    confirm.mockReturnValue(confirmed);
    await ui.click('Guardar oferta de Irion');
    expect(confirm.mock.calls[1][0].expectedOffer.price).toBe(250);
    expect(confirm.mock.calls[1][0].sourceUrl).toBe(freshSource);
  });

  it('shows list loading, errors and empty guidance without creating catalog records', async () => {
    const pending = new Subject<any>();
    getProducts.mockReturnValue(pending);
    getStores.mockReturnValue(of(envelope([])));
    const ui = await setup();
    expect(ui.dom.textContent).toContain('Cargando productos y tiendas');
    pending.next(envelope(null, false));
    pending.complete();
    await ui.fixture.whenStable();
    expect(ui.dom.textContent).toContain('No se pudieron cargar');
    expect(ui.dom.textContent).toContain('tienda activa de Irion');
    getProducts.mockReturnValue(of(envelope([])));
    await ui.click('Recargar listas');
    expect(ui.dom.textContent).toContain('No hay productos activos');
    expect(confirm).not.toHaveBeenCalled();
  });
});

describe('Contrato HTTP de importación', () => {
  it('posts exact inspection and confirmation bodies and preserves typed failure data', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(IrionOfferImportService);
    const http = TestBed.inject(HttpTestingController);
    service.inspect(selection).subscribe();
    const inspect = http.expectOne('/api/maintainer/inventories/irion/inspect');
    expect(inspect.request.method).toBe('POST');
    expect(inspect.request.body).toEqual(selection);
    inspect.flush(envelope(inspection));
    const body = { ...selection, expectedInventoryId: null, expectedRowVersion: null, expectedOffer: { currency: 'CLP', price: 200, originalPrice: null, available: false } };
    let received: unknown;
    service.confirm(body).subscribe(value => received = value);
    const confirm = http.expectOne('/api/maintainer/inventories/irion/confirm');
    expect(confirm.request.method).toBe('POST');
    expect(confirm.request.body).toEqual(body);
    const result = envelope({ outcome: 'RefreshRequired', freshPreview: preview, inspection }, false);
    confirm.flush(result);
    expect(received).toEqual(result);
    http.verify();
  });
});
