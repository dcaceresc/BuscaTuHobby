import { Injectable } from '@angular/core';
// Load dialog code and its built-in CSS only when a notification is needed.
const loadSwal = () => import('sweetalert2/dist/sweetalert2.esm.all.js')
  .then(module => module.default);

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  showSuccess(title: string, message: string) {
    void loadSwal().then(Swal => Swal.fire({
      title: title,
      text: message,
      showConfirmButton: false,
      showCloseButton: true,
      timer: 4000,
      toast: true,
      position: 'top-end',
      customClass: {
          popup: 'notificacion-success',
      }
    }));
  }

  showError(title: string, message: string) {
    void loadSwal().then(Swal => Swal.fire({
      title: title,
      text: message,
      showConfirmButton: false,
      showCloseButton: true,
      timer: 4000,
      toast: true,
      position: 'top-end',
      customClass: {
          popup: 'notificacion-error'
      }
    }));
  }

  showDefaultError() {
    this.showError('Error', 'Oh a ocurrido un error, por favor notifique al administrador del sistema');
  }

  showInvalidFormError() {
    this.showError('Error', 'Por favor complete correctamente el formulario');
  }

  confirm(title: string, message: string, confirmButtonText: string, cancelButtonText: string) {
    return loadSwal().then(Swal => Swal.fire({
      title: title,
      text: message,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: confirmButtonText,
      cancelButtonText: cancelButtonText,
      customClass: {
        popup: 'notificacion-confirm'
      }
    }));
  }
}
