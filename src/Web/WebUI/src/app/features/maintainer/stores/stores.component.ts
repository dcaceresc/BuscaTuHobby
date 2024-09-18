import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { Router, RouterLink } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { FaIconService } from '../../../core/services/fa-icon.service';
import { StoreService } from '../../../core/services/maintainer/store.service';
import { StoreDto } from '../../../core/models/maintainer/store.model';

@Component({
  selector: 'app-stores',
  standalone: true,
  imports: [
    CommonModule,ButtonComponent,TableComponent,RouterLink
  ],
  templateUrl: './stores.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoresComponent implements OnInit {

  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<StoreDto[]>([]);
  public actions: any[] = [];

  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'storeId' },
      { name: 'Nombre', key: 'storeName' },
      { name: 'Dirección', key: 'storeAddress' },
      { name: 'Sitio Web', key: 'storeWebSite' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
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
    this.router.navigate(['/maintainer/stores/update', id]);
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

}
