import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { DecimalPipe, registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { combineLatest } from 'rxjs';
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

  private slug = '';

  ngOnInit(): void {
    // La categoría y los filtros viven en la URL: cambiar cualquiera de los dos recarga desde el servidor.
    combineLatest([this.route.paramMap, this.route.queryParamMap]).subscribe(([params, queryParams]) => {
      this.slug = params.get('slug') ?? '';
      this.query.set(readQuery(queryParams));
      this.load();
    });
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
    if (page < 1 || page > this.totalPages() || page === this.query().page) {
      return;
    }
    this.updateQuery({ page });
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

  private load(): void {
    if (!this.slug) {
      this.isLoading.set(false);
      this.category.set(null);
      this.errorMessage.set('No encontramos esta categoría.');
      return;
    }

    const query = this.query();
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.dashboardService.getCategoryProducts(this.slug, {
      page: query.page,
      pageSize: PAGE_SIZE,
      sort: query.sort,
      grade: query.grade,
      manufacturerId: query.manufacturerId,
      franchiseId: query.franchiseId
    }).subscribe({
      next: (response) => {
        this.isLoading.set(false);

        if (!response.success) {
          this.category.set(null);
          this.errorMessage.set(response.message || 'No pudimos cargar los productos de esta categoría.');
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.category.set(response.data);
      },
      error: () => {
        this.isLoading.set(false);
        this.category.set(null);
        this.errorMessage.set('No pudimos cargar los productos de esta categoría.');
        this.notificationService.showDefaultError();
      }
    });
  }
}
