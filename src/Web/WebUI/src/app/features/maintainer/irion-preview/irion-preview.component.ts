import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, distinctUntilChanged, finalize, forkJoin, of } from 'rxjs';
import { ConfirmNewIrionProductOffer, NewIrionProductDetails, NewIrionProductOptionsDto, ConfirmIrionOffer, IrionOfferInspectionDto, IrionOfferSelection, IrionProductPreviewDto, ProductDto, StoreDto } from '@app/core/models';
import { IrionOfferImportService, IrionProductPreviewService, NotificationService, ProductService, StoreService } from '@app/core/services';

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
  private readonly importer = inject(IrionOfferImportService);
  private readonly productsService = inject(ProductService);
  private readonly storesService = inject(StoreService);
  private revision = 0;

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

  readonly offerForm = new FormGroup({
    productId: new FormControl('', { nonNullable: true }),
    storeId: new FormControl('', { nonNullable: true }),
    externalVariantId: new FormControl('', { nonNullable: true })
  });
  readonly products = signal<ProductDto[]>([]);
  readonly stores = signal<StoreDto[]>([]);
  readonly listsLoading = signal(false);
  readonly listsError = signal('');
  readonly operation = signal<'inspect' | 'save' | null>(null);
  readonly inspection = signal<IrionOfferInspectionDto | null>(null);
  readonly draft = signal<ConfirmIrionOffer | null>(null);
  readonly offerError = signal('');
  readonly offerStatus = signal('');

  readonly mode = new FormControl('existing', { nonNullable: true });
  readonly newForm = new FormGroup({
    productName: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)] }),
    manufacturerId: new FormControl('', { nonNullable: true, validators: Validators.required }),
    franchiseId: new FormControl('', { nonNullable: true, validators: Validators.required }),
    serieId: new FormControl('', { nonNullable: true }),
    productHasBase: new FormControl('', { nonNullable: true, validators: Validators.required }),
    productTargetAge: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)] }),
    productSize: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)] }),
    productDescription: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/\S/)] }),
    productReleaseDate: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/^(?!0001-01-01)\d{4}-\d{2}-\d{2}$/)] }),
    categoryIds: new FormControl<string[]>([], { nonNullable: true, validators: Validators.required })
  });
  readonly options = signal<NewIrionProductOptionsDto | null>(null);
  readonly optionsLoading = signal(false);
  readonly optionsError = signal('');
  private newProductId: string | null = null;
  private newDraft: ConfirmNewIrionProductOffer | null = null;

  constructor() {
    this.mode.valueChanges.pipe(takeUntilDestroyed()).subscribe(mode => {
      this.invalidate();
      if (mode === 'new') {
        this.startNewDraft();
        if (!this.options()) this.loadNewOptions();
      }
    });
    this.newForm.controls.franchiseId.valueChanges.pipe(distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(() => this.newForm.controls.serieId.setValue(''));
    this.newForm.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.invalidate());
    this.offerForm.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.invalidate());
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.invalidate());
    this.loadLists();
  }

  private startNewDraft(): void {
    if (this.newProductId) return;
    this.newProductId = crypto.randomUUID();
    this.newForm.reset({ productName: this.preview()?.title ?? '' });
  }

  loadNewOptions(): void {
    if (this.busy() || this.optionsLoading()) return;
    this.invalidate();
    this.optionsLoading.set(true);
    this.optionsError.set('');
    this.options.set(null);
    this.importer.getNewProductOptions().pipe(takeUntilDestroyed(this.destroyRef),
      finalize(() => this.optionsLoading.set(false))).subscribe({
      next: response => {
        if (response.success && response.data) {
          this.options.set(response.data);
          if (!response.data.manufacturers.length || !response.data.franchises.length || !response.data.categories.length)
            this.optionsError.set('Faltan referencias activas. Revisa el catálogo y recarga las opciones; no se crearán referencias automáticamente.');
        } else this.optionsError.set('No se pudieron cargar las opciones. Recarga las opciones para intentarlo de nuevo.');
      },
      error: () => this.optionsError.set('No se pudieron cargar las opciones. Recarga las opciones para intentarlo de nuevo.')
    });
  }

  filteredSeries() {
    return this.options()?.series.filter(s => s.franchiseId === this.newForm.controls.franchiseId.value) ?? [];
  }

  private productDetails(): NewIrionProductDetails | null {
    const p = this.newForm.getRawValue();
    const options = this.options();
    if (!this.newProductId || this.newForm.invalid || this.optionsLoading() || !options ||
      !options.manufacturers.some(m => m.id === p.manufacturerId) ||
      !options.franchises.some(f => f.id === p.franchiseId) ||
      (p.serieId !== '' && !this.filteredSeries().some(s => s.id === p.serieId)) ||
      !['yes', 'no'].includes(p.productHasBase) ||
      !p.categoryIds.length || new Set(p.categoryIds).size !== p.categoryIds.length ||
      !p.categoryIds.every(id => options.categories.some(c => c.id === id))) return null;
    return { ...p, newProductId: this.newProductId, serieId: p.serieId || null,
      productHasBase: p.productHasBase === 'yes', categoryIds: [...p.categoryIds] };
  }

  busy(): boolean { return this.loading() || this.operation() !== null; }

  loadLists(): void {
    if (this.busy() || this.listsLoading()) return;
    this.invalidate();
    this.listsLoading.set(true);
    this.listsError.set('');
    forkJoin({
      products: this.productsService.getProducts().pipe(catchError(() => of(null))),
      stores: this.storesService.getStores().pipe(catchError(() => of(null)))
    }).pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.listsLoading.set(false)))
      .subscribe(({ products, stores }) => {
        this.products.set(products?.success && products.data ? products.data.filter(p => p.isActive) : []);
        this.stores.set(stores?.success && stores.data ? stores.data.filter(s => s.isActive &&
          s.storeWebSite.length <= 2048 && s.storeWebSite === s.storeWebSite.trim() &&
          /^https:\/\/irion\.cl(?::443)?\/?$/i.test(s.storeWebSite)) : []);
        if (!products?.success || !products.data || !stores?.success || !stores.data)
          this.listsError.set('No se pudieron cargar los productos o las tiendas. Recarga las listas.');
      });
  }

  selection(): IrionOfferSelection | null {
    const values = this.offerForm.getRawValue();
    const preview = this.preview();
    if (!preview || this.listsLoading() ||
      (this.mode.value === 'new' ? !this.productDetails() : !this.products().some(p => p.productId === values.productId)) ||
      !this.stores().some(s => s.storeId === values.storeId) ||
      !preview.variants.some(v => v.id === values.externalVariantId)) return null;
    return { ...values, productId: this.mode.value === 'new' ? this.newProductId! : values.productId, sourceUrl: preview.sourceUrl };
  }

  selectedVariant() {
    return this.preview()?.variants.find(v => v.id === this.offerForm.controls.externalVariantId.value);
  }

  inspectOffer(): void {
    const selection = this.selection();
    if (this.busy() || !selection) return;
    const product = this.mode.value === 'new' ? this.productDetails() : null;
    this.invalidate();
    const revision = this.revision;
    this.operation.set('inspect');
    const request = product ? this.importer.inspectNewProduct({ product, storeId: selection.storeId,
      sourceUrl: selection.sourceUrl, externalVariantId: selection.externalVariantId }) : this.importer.inspect(selection);
    request.pipe(takeUntilDestroyed(this.destroyRef),
      finalize(() => this.operation.set(null))).subscribe({
      next: response => {
        if (revision !== this.revision) return;
        const data = response.data;
        if (!response.success || !data || !this.sameSelection(selection, data)) {
          this.offerFail(response.message || 'No se pudo revisar la vinculación. Inténtalo nuevamente.');
          return;
        }
        this.inspection.set(data);
        if (data.outcome === 'Conflict') {
          this.offerFail(product ? 'Conflicto: usar existente o revisar nombre. No se adoptará ningún producto automáticamente.' :
            data.message || 'Conflicto: la oferta existente tiene otra vinculación. No se puede guardar.');
          return;
        }
        if (data.outcome !== 'Create' && data.outcome !== 'Update') return;
        const variant = this.selectedVariant()!;
        this.draft.set({ ...selection, expectedInventoryId: data.inventoryId, expectedRowVersion: data.rowVersion,
          expectedOffer: { currency: this.preview()!.currency, price: variant.price,
            originalPrice: variant.originalPrice, available: variant.available } });
        if (product) {
          const { productId, ...snapshot } = this.draft()!;
          this.newDraft = { ...snapshot, product: structuredClone(product) };
        }
      },
      error: () => {
        if (revision === this.revision) this.offerFail('No se pudo revisar la vinculación. Inténtalo nuevamente.');
      }
    });
  }

  saveOffer(): void {
    const draft = this.draft();
    const selection = this.selection();
    if (this.busy() || !draft || !selection || !this.sameSelection(draft, selection)) return;
    const isNew = this.mode.value === 'new';
    const newDraft = this.newDraft;
    if (isNew && !newDraft) return;
    this.invalidate();
    const revision = this.revision;
    this.operation.set('save');
    const request = isNew ? this.importer.confirmNewProduct(newDraft!) : this.importer.confirm(draft);
    request.pipe(takeUntilDestroyed(this.destroyRef),
      finalize(() => this.operation.set(null))).subscribe({
      next: response => {
        if (revision !== this.revision) return;
        const data = response.data;
        if (response.success && (data?.outcome === 'Saved' || data?.outcome === 'AlreadySaved')) {
          const message = isNew ? (data.outcome === 'Saved' ? 'Producto y oferta guardados mediante confirmación conjunta.' :
            'Producto y oferta ya guardados; sin crear otro producto ni otra oferta.') :
            data.outcome === 'Saved' ? 'Oferta guardada.' : 'La oferta ya estaba guardada; no se creó un duplicado.';
          this.offerStatus.set(message);
          this.notifications.showSuccess('Oferta de Irion', message);
        } else if (data?.outcome === 'RefreshRequired') {
          if (data.freshPreview) {
            this.preview.set(data.freshPreview);
            this.form.controls.url.setValue(data.freshPreview.sourceUrl);
          }
          this.offerForm.controls.externalVariantId.setValue('');
          this.offerFail('Los datos de Irion cambiaron. Selecciona una variante y revisa de nuevo antes de confirmar. No se guardó la oferta.');
        } else if (data?.outcome === 'Conflict') {
          this.offerFail(isNew ? 'Conflicto de producto, vinculación o versión: usar existente o revisar nombre. Revisa de nuevo antes de guardar.' :
            'Conflicto de vinculación o versión. Revisa de nuevo la oferta antes de guardar.');
        } else this.offerFail(response.message || 'No se pudo guardar la oferta. Revisa de nuevo antes de confirmar.');
      },
      error: () => this.offerFail('No se pudo confirmar el resultado del guardado. Conservamos tu selección; revisa de nuevo antes de confirmar, sin reintentos automáticos.')
    });
  }

  cancelOffer(): void {
    if (this.busy()) return;
    this.offerForm.reset();
    this.newProductId = null;
    this.newForm.reset();
    if (this.mode.value === 'new') this.startNewDraft();
    this.invalidate();
    this.offerStatus.set('Borrador descartado. Cancelar no revierte ofertas ya guardadas.');
  }

  private sameSelection(a: IrionOfferSelection, b: IrionOfferSelection): boolean {
    return a.productId === b.productId && a.storeId === b.storeId && a.sourceUrl === b.sourceUrl && a.externalVariantId === b.externalVariantId;
  }

  private invalidate(): void {
    this.revision++;
    this.inspection.set(null);
    this.draft.set(null);
    this.newDraft = null;
    this.offerError.set('');
    this.offerStatus.set('');
  }

  private offerFail(message: string): void {
    this.offerError.set(message);
    this.notifications.showError('Oferta de Irion', message);
  }

  visualize(): void {
    if (this.busy()) return;
    this.invalidate();
    this.offerForm.controls.externalVariantId.setValue('');
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
