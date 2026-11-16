import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { UpdateProductComponent } from './update-product.component';
import { CategoryService, FranchiseService, ManufacturerService, NotificationService, ProductService, SerieService } from '@app/core/services';
import { ProductVM } from '@app/core/models';

const productService = { getProductById: vi.fn(), updateProduct: vi.fn() };
const notifications = { showSuccess: vi.fn(), showError: vi.fn(), showDefaultError: vi.fn() };
const router = { navigate: vi.fn() };
const envelope = <T>(data: T, success = true, message = '') => ({ success, data, message });
const listService = () => ({
  getManufacturers: () => of(envelope([])),
  getFranchises: () => of(envelope([])),
  getSeriesByFranchiseId: () => of(envelope([])),
  getCategories: () => of(envelope([]))
});

// Producto tal como lo publica la API de mantenedor: con serie, grado, escala y número de parte informados.
const storedProduct: ProductVM = {
  productId: 'p-1',
  productName: 'RX-78-2 Gundam',
  scaleId: 's-1',
  manufacturerId: 'm-1',
  franchiseId: 'f-1',
  serieId: 'se-1',
  productHasBase: false,
  productTargetAge: '8+',
  productSize: '18 cm',
  grade: 1,
  scale: '1/144',
  partNumber: 'HG-001',
  productDescription: 'Descripción',
  productReleaseDate: '2024-01-01',
  categoryIds: ['c-1'],
  productImages: []
};

/** Monta la pantalla de edición con los servicios simulados y devuelve el componente ya inicializado. */
async function setup() {
  TestBed.configureTestingModule({
    imports: [UpdateProductComponent],
    providers: [
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: 'p-1' }) } } },
      { provide: Router, useValue: router },
      { provide: ProductService, useValue: productService },
      { provide: NotificationService, useValue: notifications },
      { provide: ManufacturerService, useValue: listService() },
      { provide: FranchiseService, useValue: listService() },
      { provide: SerieService, useValue: listService() },
      { provide: CategoryService, useValue: listService() }
    ]
  });

  const fixture = TestBed.createComponent(UpdateProductComponent);
  await fixture.whenStable();
  fixture.detectChanges();

  return fixture.componentInstance;
}

describe('UpdateProductComponent', () => {
  beforeEach(() => {
    productService.getProductById.mockReset().mockReturnValue(of(envelope(storedProduct)));
    productService.updateProduct.mockReset().mockReturnValue(of(envelope(null)));
    notifications.showSuccess.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
    router.navigate.mockReset();
  });

  it('persists the form through the maintainer product service, sending grade, scale and part number', async () => {
    const component = await setup();
    component.productForm.patchValue({ grade: 2, scale: '  1/100  ', partNumber: 'MG-001' });

    component.onSubmit();

    expect(productService.updateProduct).toHaveBeenCalledTimes(1);
    expect(productService.updateProduct).toHaveBeenCalledWith('p-1', expect.objectContaining({
      productId: 'p-1',
      productName: 'RX-78-2 Gundam',
      manufacturerId: 'm-1',
      franchiseId: 'f-1',
      grade: 2,
      scale: '1/100',
      partNumber: 'MG-001',
      categoryIds: ['c-1']
    }));
    expect(notifications.showSuccess).toHaveBeenCalledWith('Exito', 'Producto actualizado correctamente');
    expect(router.navigate).toHaveBeenCalledWith(['/maintainer/products']);
  });

  it('normalises empty or blank optional text to null', async () => {
    const component = await setup();
    component.productForm.patchValue({ scale: '   ', partNumber: '' });

    component.onSubmit();

    expect(productService.updateProduct).toHaveBeenCalledWith('p-1', expect.objectContaining({
      scale: null,
      partNumber: null
    }));
  });

  it('reports a failed envelope and stays on the screen', async () => {
    productService.updateProduct.mockReturnValue(of(envelope(null, false, 'No se pudo actualizar el producto')));
    const component = await setup();

    component.onSubmit();

    expect(notifications.showError).toHaveBeenCalledWith('Error', 'No se pudo actualizar el producto');
    expect(notifications.showSuccess).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('reports a failed request with the default notification and stays on the screen', async () => {
    productService.updateProduct.mockReturnValue(throwError(() => new Error('boom')));
    const component = await setup();

    component.onSubmit();

    expect(notifications.showDefaultError).toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('does not send anything when the form is invalid', async () => {
    const component = await setup();
    component.productForm.patchValue({ productName: '' });

    component.onSubmit();

    expect(productService.updateProduct).not.toHaveBeenCalled();
    expect(notifications.showSuccess).not.toHaveBeenCalled();
    expect(notifications.showError).not.toHaveBeenCalled();
  });
});
