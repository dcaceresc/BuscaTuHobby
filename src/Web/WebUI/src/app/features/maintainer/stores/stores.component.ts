import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { StoreDto } from '@app/core/models';
import { NotificationService, StoreService } from '@app/core/services';
import { SearchComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-stores',
    imports: [TableComponent, RouterLink, SearchComponent],
    templateUrl: './stores.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class StoresComponent implements OnInit {

  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  public columns :any[] = [];
  public data = signal<StoreDto[]>([]);
  public actions: any[] = [];
  public searchTerm = signal('');

  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'storeId' },
      { name: 'Nombre', key: 'storeName' },
      { name: 'Dirección', key: 'storeAddress' },
      { name: 'Sitio Web', key: 'storeWebSite' },
      { name: 'Icono', key: 'storeIcon' },
      { name: 'Orden', key: 'storeOrder' },
      { name: 'Slug', key: 'storeSlug' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: 'bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi-toggle-on', actionKey: 'toggle'},
    ]

    this.loadStores();
  }

  public loadStores() {
    this.storeService.getStores().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.data.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onEdit(id: string) {
    this.router.navigate(['/maintainer/stores/edit', id]);
  }

  public onToggle(id: string) {
    this.storeService.toggleStore(id.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.loadStores();
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }



  public onAction(event: { id: string, actionKey: string }) {
    switch (event.actionKey) {
      case 'edit':
        this.onEdit(event.id);
        break;
      case 'toggle':
        this.onToggle(event.id);
        break;
    }
  }

  public onSearch(term: string) {
    this.searchTerm.set(term);
  }

}
