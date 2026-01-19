import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { InventoryDto } from '@app/core/models';
import { InventoryService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-inventories',
    imports: [ButtonComponent, TableComponent, RouterLink],
    templateUrl: './inventories.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoriesComponent implements OnInit {

  private inventoryService = inject(InventoryService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

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
      { icon: 'bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi-toggle-on', actionKey: 'toggle'},
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
    this.router.navigate(['/maintainer/inventories/edit', id]);
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
