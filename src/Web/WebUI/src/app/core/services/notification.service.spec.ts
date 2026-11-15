import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { NotificationService } from './notification.service';

const { fire } = vi.hoisted(() => ({ fire: vi.fn() }));
vi.mock('sweetalert2/dist/sweetalert2.esm.all.js', () => ({
  default: { fire }
}));

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    fire.mockReset();
    service = TestBed.inject(NotificationService);
  });

  it('shows the success toast with the existing dismissal options', async () => {
    expect(service.showSuccess('Guardado', 'Producto guardado')).toBeUndefined();
    await vi.dynamicImportSettled();
    expect(fire).toHaveBeenCalledExactlyOnceWith({
      title: 'Guardado', text: 'Producto guardado',
      showConfirmButton: false, showCloseButton: true, timer: 4000,
      toast: true, position: 'top-end',
      customClass: { popup: 'notificacion-success' }
    });
  });

  it('shows the error toast with the existing dismissal options', async () => {
    service.showError('Error', 'No se pudo guardar');
    await vi.dynamicImportSettled();
    expect(fire).toHaveBeenCalledExactlyOnceWith({
      title: 'Error', text: 'No se pudo guardar',
      showConfirmButton: false, showCloseButton: true, timer: 4000,
      toast: true, position: 'top-end',
      customClass: { popup: 'notificacion-error' }
    });
  });

  it('preserves default error and invalid form guidance', async () => {
    service.showDefaultError();
    service.showInvalidFormError();
    await vi.dynamicImportSettled();
    expect(fire).toHaveBeenCalledTimes(2);
    expect(fire.mock.calls.map(([options]) => options.text)).toEqual([
      'Oh a ocurrido un error, por favor notifique al administrador del sistema',
      'Por favor complete correctamente el formulario'
    ]);
  });

  it.each([true, false])('returns the confirmation result: %s', async (isConfirmed) => {
    const result = { isConfirmed, isDenied: false, isDismissed: !isConfirmed };
    fire.mockResolvedValue(result);
    const pending = service.confirm('Eliminar', '¿Confirmas?', 'Sí', 'No');
    await vi.dynamicImportSettled();
    await expect(pending).resolves.toEqual(result);
    expect(fire).toHaveBeenCalledExactlyOnceWith({
      title: 'Eliminar', text: '¿Confirmas?', icon: 'warning',
      showCancelButton: true, confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33', confirmButtonText: 'Sí', cancelButtonText: 'No',
      customClass: { popup: 'notificacion-confirm' }
    });
  });
});
