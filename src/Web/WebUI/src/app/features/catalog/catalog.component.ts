import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DecimalPipe, registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { ActivatedRoute, NavigationEnd, ParamMap, Router, RouterLink } from '@angular/router';
import { EMPTY, Observable, catchError, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { ApiResponse } from '@app/core/models/apiResponse.model';
import { CatalogProductDto, CategoryProductsDto } from '@app/core/models/dashboard/dashboard.model';
import { gunplaGradeLabel } from '@app/core/models';
import { DashboardService, NotificationService } from '@app/core/services';

// Los precios se formatean en locale chileno; los datos de locale viajan con este chunk diferido.
registerLocaleData(localeEsCl, 'es-CL');

// El orden viaja a la API con los mismos valores que declara su contrato; el defecto es por nombre.
const SORT_OPTIONS = [
  { value: 'name', label: 'Nombre' },
  { value: 'price-asc', label: 'Menor precio' },
  { value: 'price-desc', label: 'Mayor precio' },
  { value: 'views', label: 'Más vistos' }
] as const;

const SORT_VALUES: readonly string[] = SORT_OPTIONS.map(option => option.value);

// El tamaño de página viaja también en la URL indirectamente: la página pedida se resuelve con este tamaño.
const PAGE_SIZE = 12;

/** Estado del catálogo que la URL conserva para que una recarga o un enlace reproduzcan la misma vista. */
interface CatalogQuery {
  page: number;
  sort: string;
  grade: string;
  manufacturerId: string;
  franchiseId: string;
}

/** Estado del catálogo para una navegación: el slug de la categoría y los filtros que publica la URL. */
interface CatalogState {
  slug: string;
  query: CatalogQuery;
}

/** Lee el estado desde la URL: valores desconocidos vuelven al defecto en lugar de romper la vista. */
function readQuery(params: ParamMap): CatalogQuery {
  const page = Number(params.get('page'));
  const sort = params.get('sort') ?? '';

  return {
    page: Number.isInteger(page) && page > 0 ? page : 1,
    sort: SORT_VALUES.includes(sort) ? sort : 'name',
    grade: params.get('grade') ?? '',
    manufacturerId: params.get('manufacturerId') ?? '',
    franchiseId: params.get('franchiseId') ?? ''
  };
}

/**
 * Dos navegaciones con el mismo slug y los mismos filtros describen la misma vista y no se repiten: la comparación
 * evita pedir de nuevo lo que ya está en pantalla.
 */
function sameState(previous: CatalogState, current: CatalogState): boolean {
  return previous.slug === current.slug
    && previous.query.page === current.query.page
    && previous.query.sort === current.query.sort
    && previous.query.grade === current.query.grade
    && previous.query.manufacturerId === current.query.manufacturerId
    && previous.query.franchiseId === current.query.franchiseId;
}

@Component({
  selector: 'app-catalog',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CatalogComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  public category = signal<CategoryProductsDto | null>(null);
  public query = signal<CatalogQuery>({ page: 1, sort: 'name', grade: '', manufacturerId: '', franchiseId: '' });
  public isLoading = signal(true);
  public errorMessage = signal('');
  public readonly sortOptions = SORT_OPTIONS;

  /** Cantidad de páginas que declara la API; siempre al menos una para poder mostrar la vista vacía. */
  public totalPages = computed(() => {
    const category = this.category();
    if (!category || category.pageSize <= 0) {
      return 1;
    }
    return Math.max(1, Math.ceil(category.total / category.pageSize));
  });

  public pages = computed(() => Array.from({ length: this.totalPages() }, (_, index) => index + 1));

  ngOnInit(): void {
    // La URL es la única fuente del estado del catálogo. Se escucha el final de cada navegación, y no la combinación
    // de paramMap y queryParamMap: esa combinación emitía dos veces cuando una navegación cambiaba a la vez el slug y
    // los filtros, y lanzaba dos peticiones. Así cada navegación produce una sola petición, y switchMap descarta la
    // respuesta de la categoría anterior para que nunca sobrescriba la vigente.
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        map(() => this.readState()),
        startWith(this.readState()),
        distinctUntilChanged(sameState),
        switchMap(state => {
          this.beginLoad(state);
          return this.requestFor(state).pipe(catchError(() => of(null)));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => this.finishLoad(response));
  }

  /** Etiqueta del grado Gunpla, o cadena vacía cuando el producto no tiene grado asignado. */
  public gradeLabel(grade: number): string {
    return grade > 0 ? gunplaGradeLabel(grade) : '';
  }

  /** Primera imagen del producto; sin imágenes publicadas se usa el placeholder compartido. */
  public imageFor(product: CatalogProductDto): string {
    return product.imageUrls[0] ?? 'assets/images/placeholder.svg';
  }

  public onManufacturerChange(event: Event): void {
    this.updateQuery({ manufacturerId: (event.target as HTMLSelectElement).value });
  }

  public onFranchiseChange(event: Event): void {
    this.updateQuery({ franchiseId: (event.target as HTMLSelectElement).value });
  }

  public onGradeChange(event: Event): void {
    this.updateQuery({ grade: (event.target as HTMLSelectElement).value });
  }

  public onSortChange(event: Event): void {
    this.updateQuery({ sort: (event.target as HTMLSelectElement).value });
  }

  public onPageChange(page: number): void {
    // Una página fuera de rango no se puede alcanzar con el paginador; la alerta de página inexistente ofrece el
    // regreso a la primera página por este mismo método.
    if (page < 1 || page > this.totalPages() || page === this.query().page) {
      return;
    }
    this.updateQuery({ page });
  }

  /**
   * Una categoría con productos puede no tener ninguno en la página pedida: el total real es mayor que cero y la
   * página llega vacía. Es un caso distinto de los filtros que no dejan ningún producto.
   */
  public isOutOfRange(data: CategoryProductsDto): boolean {
    return data.items.length === 0 && data.total > 0;
  }

  public clearFilters(): void {
    this.updateQuery({ grade: '', manufacturerId: '', franchiseId: '' });
  }

  /**
   * Cambia el estado del catálogo y lo deja completo en la URL. Un filtro o un orden nuevo vuelven a la
   * primera página; el paginador es el único que envía su propia página.
   */
  private updateQuery(changes: Partial<CatalogQuery>): void {
    const next: CatalogQuery = { ...this.query(), ...changes, page: changes.page ?? 1 };

    this.query.set(next);
    void this.router.navigate([], { relativeTo: this.route, queryParams: this.toQueryParams(next) });
  }

  /** Escribe en la URL solo lo que se aparta del defecto, para que un enlace compartido quede corto y estable. */
  private toQueryParams(query: CatalogQuery): Record<string, string> {
    const params: Record<string, string> = {};

    if (query.page > 1) {
      params['page'] = String(query.page);
    }
    if (query.sort !== 'name') {
      params['sort'] = query.sort;
    }
    if (query.grade) {
      params['grade'] = query.grade;
    }
    if (query.manufacturerId) {
      params['manufacturerId'] = query.manufacturerId;
    }
    if (query.franchiseId) {
      params['franchiseId'] = query.franchiseId;
    }

    return params;
  }

  /** Estado que publica la URL en este instante; se lee en cada navegación terminada y una vez al iniciar. */
  private readState(): CatalogState {
    return {
      slug: this.route.snapshot.paramMap.get('slug') ?? '',
      query: readQuery(this.route.snapshot.queryParamMap)
    };
  }

  /** Deja la vista en carga y publica el estado de la nueva navegación antes de pedir sus productos. */
  private beginLoad(state: CatalogState): void {
    this.query.set(state.query);

    if (!state.slug) {
      this.isLoading.set(false);
      this.category.set(null);
      this.errorMessage.set('No encontramos esta categoría.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');
  }

  /** Petición del estado pedido; sin slug no hay nada que pedir y la vista ya quedó con su mensaje. */
  private requestFor(state: CatalogState): Observable<ApiResponse<CategoryProductsDto> | null> {
    if (!state.slug) {
      return EMPTY;
    }

    const query = state.query;

    return this.dashboardService.getCategoryProducts(state.slug, {
      page: query.page,
      pageSize: PAGE_SIZE,
      sort: query.sort,
      grade: query.grade,
      manufacturerId: query.manufacturerId,
      franchiseId: query.franchiseId
    });
  }

  /** Aplica la respuesta vigente; un null significa que la petición falló en el transporte. */
  private finishLoad(response: ApiResponse<CategoryProductsDto> | null): void {
    this.isLoading.set(false);

    if (response === null) {
      this.category.set(null);
      this.errorMessage.set('No pudimos cargar los productos de esta categoría.');
      this.notificationService.showDefaultError();
      return;
    }

    if (!response.success) {
      this.category.set(null);
      this.errorMessage.set(response.message || 'No pudimos cargar los productos de esta categoría.');
      this.notificationService.showError('Error', response.message);
      return;
    }

    this.category.set(response.data);
  }
}
