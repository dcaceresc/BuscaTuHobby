import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { ManufacturerDto } from '../../../core/models/maintainer/manufacturer.model';
import { FaIconService } from '../../../core/services/fa-icon.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ManufacturerService } from '../../../core/services/maintainer/manufacturer.service';

@Component({
  selector: 'app-manufacturers',
  standalone: true,
  imports: [
    CommonModule,ButtonComponent,TableComponent,RouterLink
  ],
  templateUrl: './manufacturers.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ManufacturersComponent implements OnInit {

  private manufacturerService = inject(ManufacturerService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

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
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
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
    this.router.navigate(['/maintainer/manufacturers/update', id]);
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
