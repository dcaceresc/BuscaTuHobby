import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { InventoryService } from '../../../core/services/maintainer/inventory.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Router, RouterLink } from '@angular/router';
import { FaIconService } from '../../../core/services/fa-icon.service';
import { InventoryDto } from '../../../core/models/maintainer/inventory.model';

@Component({
  selector: 'app-inventories',
  standalone: true,
  imports: [
    CommonModule,ButtonComponent,TableComponent,RouterLink
  ],
  templateUrl: './inventories.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InventoriesComponent implements OnInit {

  private inventoryService = inject(InventoryService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<InventoryDto[]>([]);
  public actions: any[] = [];

  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'inventoryId' },
      { name: 'Producto', key: 'productName' },
      { name: 'Tienda', key: 'storeName' },
      { name: 'Precio', key: 'price' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ];

    this.loadInventories();

  }

  public loadInventories(){
    this.inventoryService.getInventories().subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error", response.message);
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
    this.router.navigate(['/maintainer/inventories/update', id]);
  }

  public onToggle(id: string) {
    this.inventoryService.toggleInventory(id).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Exito", response.message);

        this.loadInventories();
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
