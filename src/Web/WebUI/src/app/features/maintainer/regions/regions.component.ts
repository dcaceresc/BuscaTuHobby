import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { RegionDto } from '@app/core/models';
import { FaIconService, NotificationService, RegionService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-regions',
    imports: [
        CommonModule, ButtonComponent, TableComponent, RouterLink
    ],
    templateUrl: './regions.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegionsComponent implements OnInit { 

  private regionService = inject(RegionService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<RegionDto[]>([]);
  public actions: any[] = [];

  public ngOnInit() : void {
    this.columns = [
      { name: '#', key: 'regionId' },
      { name: 'Nombre', key: 'regionName' },
      { name: 'Orden', key: 'regionOrder' },
      { name: 'Acciones', key: 'isActive' }
    ];
    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ];

    this.getRegions();
  }

  public getRegions(){
    this.regionService.getRegions().subscribe({
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
    });
  }

  public onEdit(id: string) {
    this.router.navigate(['/maintainer/regions/update', id]);
  }

  public onToggle(id: string) {
    this.regionService.toggleRegion(id).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.getRegions();
        this.notificationService.showSuccess('Exito', response.message);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
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
