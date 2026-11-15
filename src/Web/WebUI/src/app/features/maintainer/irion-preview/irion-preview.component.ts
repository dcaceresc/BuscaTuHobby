import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { IrionProductPreviewDto } from '@app/core/models';
import { IrionProductPreviewService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-irion-preview',
  imports: [ReactiveFormsModule],
  templateUrl: './irion-preview.component.html',
  styleUrl: './irion-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IrionPreviewComponent {
  private readonly service = inject(IrionProductPreviewService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = new FormGroup({
    url: new FormControl('', { nonNullable: true, validators: [
      Validators.required,
      Validators.maxLength(8192),
      Validators.pattern(/^https:\/\/irion\.cl(?::443)?\/(?:collections\/[a-z0-9]+(?:-[a-z0-9]+)*\/)?products\/[a-z0-9]+(?:-[a-z0-9]+)*\/?(?:[?#].*)?$/i)
    ] })
  });
  readonly preview = signal<IrionProductPreviewDto | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');

  visualize(): void {
    if (this.loading()) return;
    this.form.controls.url.setValue(this.form.controls.url.value.trim());
    this.form.markAllAsTouched();
    this.preview.set(null);
    this.error.set('');
    if (this.form.invalid) {
      this.error.set('Introduce una URL válida de Irion: https://irion.cl/products/nombre-del-producto o https://irion.cl/collections/coleccion/products/nombre-del-producto');
      return;
    }

    this.loading.set(true);
    this.service.preview(this.form.controls.url.value).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: response => {
        if (response.success && response.data) this.preview.set(response.data);
        else this.fail(response.message || 'No se pudo obtener la vista previa de Irion.');
      },
      error: () => this.fail('No se pudo consultar Irion. Inténtalo nuevamente.')
    });
  }

  formatPrice(value: number, currency: string): string {
    return new Intl.NumberFormat('es-CL', {
      style: 'currency', currency, minimumFractionDigits: 0, maximumFractionDigits: 2
    }).format(value);
  }

  private fail(message: string): void {
    this.error.set(message);
    this.notifications.showError('Vista previa de Irion', message);
  }
}
