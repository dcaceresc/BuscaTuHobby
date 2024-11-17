import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { InventoryDto } from '@app/core/models';
import { FaIconService, InventoryService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
  selector: 'app-inventories',
  standalone: true,
  imports: [ButtonComponent,TableComponent,RouterLink],
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
