import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, takeUntil } from 'rxjs';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';
import { SearchProductDto } from '@app/core/models/dashboard/dashboard.model';

@Component({
  selector: 'app-search-product',
  imports: [DecimalPipe],
  templateUrl: './search-product.component.html',
  styleUrl: './search-product.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchProductComponent implements OnInit, OnDestroy {
  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public searchTerm = signal('');
  public results = signal<SearchProductDto[]>([]);
  public isLoading = signal(false);
  public hasSearched = signal(false);

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term.trim()) {
          this.results.set([]);
          this.hasSearched.set(false);
          this.isLoading.set(false);
          return of(null);
        }
        this.isLoading.set(true);
        return this.dashboardService.searchProducts(term);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        if (response === null) return;
        this.isLoading.set(false);
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.results.set(response.data);
        this.hasSearched.set(true);
      },
      error: () => {
        this.isLoading.set(false);
        this.notificationService.showDefaultError();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onInput(event: Event): void {
    const term = (event.target as HTMLInputElement).value;
    this.searchTerm.set(term);
    this.searchSubject.next(term);
  }

  onSearch(): void {
    const term = this.searchTerm();
    if (term.trim()) {
      this.isLoading.set(true);
      this.searchSubject.next(term);
    }
  }

  clear(): void {
    this.searchTerm.set('');
    this.results.set([]);
    this.hasSearched.set(false);
    this.isLoading.set(false);
  }
}
