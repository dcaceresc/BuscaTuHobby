import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of, Subject } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { IrionPreviewComponent } from './irion-preview.component';
import { ConfirmNewIrionProductOffer, InspectNewIrionProductOffer, NewIrionProductOptionsDto } from '@app/core/models';
import { IrionOfferImportService, IrionProductPreviewService, NotificationService, ProductService, StoreService } from '@app/core/services';

const sourceUrl = 'https://irion.cl/products/model';
const preview = { sourceUrl, title: '<b>Modelo</b>', vendor: '', currency: 'CLP', descriptionHtml: '<script>source</script>', tags: [], imageUrls: [], available: true,
  variants: [{ id: '456', sku: null, barcode: null, price: 200, originalPrice: null, available: false, discountPercentage: null }] };
const options: NewIrionProductOptionsDto = {
  manufacturers: [{ id: 'm1', label: 'Fabricante activo' }], franchises: [{ id: 'f1', label: 'Franquicia A' }, { id: 'f2', label: 'Franquicia B' }],
  categories: [{ id: 'c1', label: 'Categoría A' }, { id: 'c2', label: 'Categoría B' }],
  series: [{ id: 'serie1', label: 'Serie A', franchiseId: 'f1' }, { id: 'serie2', label: 'Serie B', franchiseId: 'f2' }]
};
const envelope = <T>(data: T, success = true, message = '') => ({ success, data, message });

describe('Producto nuevo explícito de Irion', () => {
  const inspectNewProduct = vi.fn(), confirmNewProduct = vi.fn(), getNewProductOptions = vi.fn();
  const createProduct = vi.fn(), upload = vi.fn(), oldInspect = vi.fn(), oldConfirm = vi.fn();
  let inspected: Subject<any>, confirmed: Subject<any>;
  beforeEach(async () => {
    inspected = new Subject(); confirmed = new Subject();
    inspectNewProduct.mockReset().mockReturnValue(inspected);
    confirmNewProduct.mockReset().mockReturnValue(confirmed);
    getNewProductOptions.mockReset().mockReturnValue(of(envelope(options)));
    [createProduct, upload, oldInspect, oldConfirm].forEach(mock => mock.mockReset());
    await TestBed.configureTestingModule({ imports: [IrionPreviewComponent], providers: [
      { provide: IrionProductPreviewService, useValue: { preview: () => of(envelope(preview)) } },
      { provide: IrionOfferImportService, useValue: { getNewProductOptions, inspectNewProduct, confirmNewProduct, inspect: oldInspect, confirm: oldConfirm } },
      { provide: ProductService, useValue: { getProducts: () => of(envelope([])), createProduct, upload } },
      { provide: StoreService, useValue: { getStores: () => of(envelope([{ storeId: 's1', storeName: 'Irion', storeWebSite: 'https://irion.cl', isActive: true }])) } },
      { provide: NotificationService, useValue: { showSuccess: vi.fn(), showError: vi.fn() } }
    ] }).compileComponents();
  });

  async function setup() {
    const fixture = TestBed.createComponent(IrionPreviewComponent);
    await fixture.whenStable();
    const dom = fixture.nativeElement as HTMLElement;
    const change = async (id: string, value: string | string[]) => {
      const input = dom.querySelector(`#${id}`) as HTMLInputElement | HTMLSelectElement;
      if (Array.isArray(value)) {
        const labels = options.categories.filter(c => value.includes(c.id)).map(c => c.label);
        [...(input as HTMLSelectElement).options].forEach(o => o.selected = labels.includes(o.textContent ?? ''));
      }
      else input.value = value;
      input.dispatchEvent(new Event(input.tagName === 'SELECT' ? 'change' : 'input'));
      await fixture.whenStable();
    };
    const click = async (text: string) => {
      const button = [...dom.querySelectorAll('button')].find(b => b.textContent?.includes(text))!;
      button.click(); await fixture.whenStable(); return button;
    };
    await change('irion-url', sourceUrl); await click('Visualizar producto');
    expect(getNewProductOptions).not.toHaveBeenCalled();
    await change('irion-mode', 'new');
    const fill = async () => {
      for (const [id, value] of Object.entries({ manufacturerId: 'm1', franchiseId: 'f1', productTargetAge: '15+', productSize: '20 cm', productDescription: '<b>Texto humano</b>', productReleaseDate: '2020-01-02', productHasBase: 'no' }))
        await change(`new-${id}`, value);
      await change('new-categoryIds', ['c1', 'c2']);
      await change('irion-store', 's1'); await change('irion-variant', '456');
    };
    const review = async (outcome = 'Create') => {
      await click('Revisar vinculación');
      const request = inspectNewProduct.mock.lastCall![0] as InspectNewIrionProductOffer;
      inspected.next(envelope({ ...request, productId: request.product.newProductId, outcome, inventoryId: outcome === 'Update' ? 'inventory' : null, rowVersion: outcome === 'Update' ? 'AQIDBAUGBwg=' : null, isActive: null, message: null }));
      inspected.complete(); await fixture.whenStable(); return request;
    };
    return { fixture, dom, change, click, fill, review };
  }

  it('loads only active reference options on demand; requires every field without invented metadata, accepts image-free text', async () => {
    const ui = await setup();
    expect(getNewProductOptions).toHaveBeenCalledTimes(1);
    expect(ui.dom.textContent).toContain('imágenes son opcionales');
    expect((ui.dom.querySelector('#new-productName') as HTMLInputElement).value).toBe(preview.title);
    for (const id of ['manufacturerId', 'franchiseId', 'serieId', 'productHasBase', 'productReleaseDate', 'productDescription'])
      expect((ui.dom.querySelector(`#new-${id}`) as HTMLInputElement).value).toBe('');
    expect((ui.dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('');
    expect((await ui.click('Revisar vinculación')).disabled).toBe(true);
    await ui.fill();
    for (const id of ['productName', 'manufacturerId', 'franchiseId', 'productHasBase', 'productTargetAge', 'productSize', 'productDescription', 'productReleaseDate']) {
      const input = ui.dom.querySelector(`#new-${id}`) as HTMLInputElement;
      const value = input.value;
      await ui.change(`new-${id}`, '');
      expect((await ui.click('Revisar vinculación')).disabled).toBe(true);
      await ui.change(`new-${id}`, value);
    }
    await ui.change('new-categoryIds', []);
    expect((await ui.click('Revisar vinculación')).disabled).toBe(true);
    await ui.change('new-categoryIds', ['c1', 'c2']);
    for (const [id, max] of [['productName', 200], ['productTargetAge', 100], ['productSize', 100]] as const) {
      const value = (ui.dom.querySelector(`#new-${id}`) as HTMLInputElement).value;
      await ui.change(`new-${id}`, 'x'.repeat(max + 1));
      expect((await ui.click('Revisar vinculación')).disabled).toBe(true);
      await ui.change(`new-${id}`, value);
    }
    const request = await ui.review();
    expect(request.product).toEqual({ newProductId: expect.stringMatching(/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i), productName: preview.title, manufacturerId: 'm1', franchiseId: 'f1', serieId: null, productHasBase: false, productTargetAge: '15+', productSize: '20 cm', productDescription: '<b>Texto humano</b>', productReleaseDate: '2020-01-02', categoryIds: ['c1', 'c2'] });
    expect(ui.dom.querySelector('script')).toBeNull();
    expect(confirmNewProduct).not.toHaveBeenCalled();
    expect(createProduct).not.toHaveBeenCalled(); expect(upload).not.toHaveBeenCalled();
  });

  it('filters series by franchise and clears only on franchise change', async () => {
    const ui = await setup(); await ui.fill();
    expect([...ui.dom.querySelectorAll('#new-serieId option')].map(o => o.getAttribute('value'))).toEqual(['', 'serie1']);
    await ui.change('new-serieId', 'serie1'); await ui.change('new-productSize', '25 cm');
    expect((ui.dom.querySelector('#new-serieId') as HTMLSelectElement).value).toBe('serie1');
    await ui.change('new-franchiseId', 'f2');
    expect((ui.dom.querySelector('#new-serieId') as HTMLSelectElement).value).toBe('');
    expect([...ui.dom.querySelectorAll('#new-serieId option')].map(o => o.getAttribute('value'))).toEqual(['', 'serie2']);
  });

  it.each(['Saved', 'AlreadySaved'])('posts one atomic snapshot with inspected version, announces %s and never uses old creation/upload endpoints', async outcome => {
    const ui = await setup(); await ui.fill(); const request = await ui.review('Update');
    await ui.click('Crear producto y guardar oferta'); await ui.click('Crear producto y guardar oferta');
    expect(confirmNewProduct).toHaveBeenCalledExactlyOnceWith({ ...request, expectedInventoryId: 'inventory', expectedRowVersion: 'AQIDBAUGBwg=', expectedOffer: { currency: 'CLP', price: 200, originalPrice: null, available: false } });
    expect(ui.dom.querySelector('#new-productName')?.matches(':disabled')).toBe(true);
    expect(ui.dom.querySelector('#irion-mode')?.matches(':disabled')).toBe(true);
    confirmed.next(envelope({ outcome })); confirmed.complete(); await ui.fixture.whenStable();
    expect(ui.dom.textContent).toContain(outcome === 'Saved' ? 'Producto y oferta guardados' : 'sin crear otro producto ni otra oferta');
    expect((await ui.click('Crear producto y guardar oferta')).disabled).toBe(true);
    for (const mock of [createProduct, upload, oldInspect, oldConfirm]) expect(mock).not.toHaveBeenCalled();
    expect(confirmNewProduct).toHaveBeenCalledTimes(1);
  });

  it.each(['timeout', '429', 'Conflict'])('keeps a stable UUID and human data through %s and manual reinspection', async failure => {
    const ui = await setup(); await ui.fill(); const original = await ui.review(); await ui.click('Crear producto y guardar oferta');
    if (failure === 'timeout') confirmed.error(new Error('timeout'));
    else { confirmed.next(envelope(failure === 'Conflict' ? { outcome: 'Conflict' } : null, false, 'HTTP 429: espera')); confirmed.complete(); }
    await ui.fixture.whenStable();
    expect((await ui.click('Crear producto y guardar oferta')).disabled).toBe(true);
    expect(ui.dom.querySelector('[role="alert"]')).not.toBeNull();
    expect(ui.dom.textContent).toContain(failure === 'Conflict' ? 'usar existente o revisar nombre' : failure === '429' ? 'HTTP 429' : 'sin reintentos automáticos');
    inspected = new Subject(); inspectNewProduct.mockReturnValue(inspected);
    const retried = await ui.review('Update');
    expect(retried).toEqual(original);
    expect(confirmNewProduct).toHaveBeenCalledTimes(1);
    confirmed = new Subject(); confirmNewProduct.mockReturnValue(confirmed);
    await ui.click('Crear producto y guardar oferta');
    expect(confirmNewProduct.mock.calls[1][0].product.newProductId).toBe(original.product.newProductId);
    expect(confirmNewProduct.mock.calls[1][0].expectedRowVersion).toBe('AQIDBAUGBwg=');
  });

  it.each([['new-productName', 'Otro'], ['new-categoryIds', ['c2']], ['irion-mode', 'existing'], ['irion-url', 'https://irion.cl/products/other']])('disarms inspected data after %s changes', async (id, value) => {
    const ui = await setup(); await ui.fill(); await ui.review(); await ui.change(id as string, value);
    expect((await ui.click(id === 'irion-mode' ? 'Guardar oferta de Irion' : 'Crear producto y guardar oferta')).disabled).toBe(true);
    expect(confirmNewProduct).not.toHaveBeenCalled();
  });

  it.each([['irion-mode', 'existing'], ['new-productName', 'Changed'], ['new-categoryIds', ['c2']], ['new-franchiseId', 'f2'], ['irion-url', 'https://irion.cl/products/other']])('rejects stale inspection replies after %s changes without overlapping operations', async (id, value) => {
    const ui = await setup(); await ui.fill(); await ui.click('Revisar vinculación'); await ui.click('Revisar vinculación');
    const request = inspectNewProduct.mock.lastCall![0];
    await ui.change(id as string, value);
    inspected.next(envelope({ ...request, productId: request.product.newProductId, outcome: 'Create' })); inspected.complete(); await ui.fixture.whenStable();
    expect(inspectNewProduct).toHaveBeenCalledTimes(1);
    expect((await ui.click(id === 'irion-mode' ? 'Guardar oferta de Irion' : 'Crear producto y guardar oferta')).disabled).toBe(true);
    expect(confirmNewProduct).not.toHaveBeenCalled(); expect(oldConfirm).not.toHaveBeenCalled();
  });

  it('requires manual reselection/reinspection/save on RefreshRequired without losing draft ID or human metadata', async () => {
    const ui = await setup(); await ui.fill(); const original = await ui.review(); await ui.click('Crear producto y guardar oferta');
    const fresh = { ...preview, title: 'Título cambiado', variants: [{ ...preview.variants[0], price: 250 }] };
    confirmed.next(envelope({ outcome: 'RefreshRequired', freshPreview: fresh }, false)); confirmed.complete(); await ui.fixture.whenStable();
    expect((ui.dom.querySelector('#irion-variant') as HTMLSelectElement).value).toBe('');
    expect((ui.dom.querySelector('#new-productName') as HTMLInputElement).value).toBe(preview.title);
    expect((await ui.click('Crear producto y guardar oferta')).disabled).toBe(true);
    expect(confirmNewProduct).toHaveBeenCalledTimes(1);
    await ui.change('irion-variant', '456'); inspected = new Subject(); inspectNewProduct.mockReturnValue(inspected);
    expect((await ui.review()).product).toEqual(original.product);
    confirmed = new Subject(); confirmNewProduct.mockReturnValue(confirmed); await ui.click('Crear producto y guardar oferta');
    expect(confirmNewProduct.mock.calls[1][0].expectedOffer.price).toBe(250);
  });

  it.each(['error', 'empty'])('recovers from %s options without blocking read-only preview or existing mode', async kind => {
    getNewProductOptions.mockReturnValue(kind === 'error' ? of(envelope(null, false)) : of(envelope({ ...options, manufacturers: [] })));
    const ui = await setup();
    expect(ui.dom.textContent).toContain(kind === 'error' ? 'No se pudieron cargar' : 'Faltan referencias activas');
    expect((await ui.click('Revisar vinculación')).disabled).toBe(true);
    await ui.change('irion-mode', 'existing');
    expect((await ui.click('Visualizar producto')).disabled).toBe(false);
    await ui.change('irion-mode', 'new');
    getNewProductOptions.mockReturnValue(of(envelope(options))); await ui.click('Recargar opciones'); await ui.fill();
    expect([...ui.dom.querySelectorAll('button')].find(b => b.textContent?.includes('Revisar vinculación'))!.disabled).toBe(false);
    await ui.click('Revisar vinculación');
    expect(inspectNewProduct).toHaveBeenCalledTimes(1);
  });

  it('shows loading and recovers from a transport options failure without losing the preview', async () => {
    const pending = new Subject<any>();
    getNewProductOptions.mockReturnValue(pending);
    const ui = await setup();
    expect(ui.dom.textContent).toContain('Cargando opciones activas');
    expect(ui.dom.querySelector('#new-productName')?.matches(':disabled')).toBe(true);
    pending.error(new Error('offline')); await ui.fixture.whenStable();
    expect(ui.dom.textContent).toContain('No se pudieron cargar');
    expect(ui.dom.querySelector('pre')?.textContent).toBe(preview.descriptionHtml);
    getNewProductOptions.mockReturnValue(of(envelope(options)));
    await ui.click('Recargar opciones'); await ui.fill(); await ui.review();
    expect(inspectNewProduct).toHaveBeenCalledTimes(1);
    expect(confirmNewProduct).not.toHaveBeenCalled();
  });

  it('rejects a mismatched product ID/conflict, preserves ID across edits and starts a new ID only on explicit cancellation', async () => {
    const ui = await setup(); await ui.fill(); await ui.click('Revisar vinculación');
    const original = inspectNewProduct.mock.lastCall![0];
    inspected.next(envelope({ ...original, productId: 'other', outcome: 'Create' })); inspected.complete(); await ui.fixture.whenStable();
    expect((await ui.click('Crear producto y guardar oferta')).disabled).toBe(true);
    await ui.change('new-productName', 'Otro nombre'); inspected = new Subject(); inspectNewProduct.mockReturnValue(inspected);
    const changed = await ui.review('Conflict');
    expect(changed.product.newProductId).toBe(original.product.newProductId);
    expect(ui.dom.textContent).toContain('usar existente o revisar nombre');
    expect((await ui.click('Crear producto y guardar oferta')).disabled).toBe(true);
    await ui.click('Cancelar borrador'); await ui.fill(); inspected = new Subject(); inspectNewProduct.mockReturnValue(inspected);
    expect((await ui.review()).product.newProductId).not.toBe(original.product.newProductId);
    expect(confirmNewProduct).not.toHaveBeenCalled();
  });
});

describe('Contrato HTTP producto nuevo Irion', () => {
  it('gets typed options and posts exact new-product inspection and atomic confirmation bodies', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(IrionOfferImportService), http = TestBed.inject(HttpTestingController);
    service.getNewProductOptions().subscribe(value => expect(value.data).toEqual(options));
    const get = http.expectOne('/api/maintainer/inventories/irion/new-product/options');
    expect(get.request.method).toBe('GET'); get.flush(envelope(options));
    const request: InspectNewIrionProductOffer = { product: { newProductId: '11111111-1111-4111-8111-111111111111', productName: 'Nombre', manufacturerId: 'm1', franchiseId: 'f1', serieId: null, productHasBase: false, productTargetAge: '15+', productSize: '20 cm', productDescription: '<b>texto</b>', productReleaseDate: '2020-01-02', categoryIds: ['c1'] }, storeId: 's1', sourceUrl, externalVariantId: '456' };
    service.inspectNewProduct(request).subscribe();
    const inspection = http.expectOne('/api/maintainer/inventories/irion/new-product/inspect');
    expect(inspection.request.method).toBe('POST'); expect(inspection.request.body).toEqual(request); inspection.flush(envelope(null));
    const body: ConfirmNewIrionProductOffer = { ...request, expectedInventoryId: 'inventory', expectedRowVersion: 'AQIDBAUGBwg=', expectedOffer: { currency: 'CLP', price: 200, originalPrice: null, available: false } };
    let received: unknown; service.confirmNewProduct(body).subscribe(value => received = value);
    const confirm = http.expectOne('/api/maintainer/inventories/irion/new-product/confirm');
    expect(confirm.request.method).toBe('POST'); expect(confirm.request.body).toEqual(body);
    const result = envelope({ outcome: 'RefreshRequired', freshPreview: preview, inspection: null }, false);
    confirm.flush(result); expect(received).toEqual(result); http.verify();
  });
});
