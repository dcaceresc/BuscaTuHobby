
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ManufacturerDto } from '@app/core/models';
import { ManufacturerService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';
@Component({
    selector: 'app-manufacturers',
    imports: [ButtonComponent, TableComponent, RouterLink],
    templateUrl: './manufacturers.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManufacturersComponent implements OnInit {

  private manufacturerService = inject(ManufacturerService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  public columns :any[] = [];
  public data = signal<ManufacturerDto[]>([]);
  public actions: any[] = [];

  public ngOnInit() {
    this.columns = [
      { name: '#', key: 'manufacturerId' },
      { name: 'Nombre', key: 'manufacturerName' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: 'bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi-toggle-on', actionKey: 'toggle'},
    ]

    this.loadManufacturers();
  }


  public loadManufacturers() {
    this.manufacturerService.getManufacturers().subscribe({
      next: (response) => {
        
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.data.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    })
  }


  public onEdit(id: string) {
    this.router.navigate(['/maintainer/manufacturers/edit', id]);
  }

  public onToggle(id: string) {
    this.manufacturerService.toggleManufacturer(id.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.loadManufacturers();
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
