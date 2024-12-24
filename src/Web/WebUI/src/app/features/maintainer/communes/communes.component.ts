import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommuneDto } from '@app/core/models';
import { CommuneService, FaIconService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-communes',
    imports: [ButtonComponent, TableComponent, RouterLink],
    templateUrl: './communes.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommunesComponent implements OnInit {

  private communeService = inject(CommuneService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<CommuneDto[]>([]);
  public actions: any[] = [];


  public ngOnInit() : void{
    this.columns = [
      { name: '#', key: 'communeId' },
      { name: 'Nombre', key: 'communeName' },
      { name: 'Region', key: 'regionName' },
      { name: 'Acciones', key: 'isActive' }
    ];
    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ];
    this.loadCommunes();
  }

  public loadCommunes() {
    this.communeService.getCommunes().subscribe({
      next: (response) => {
        this.data.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onEdit(id: string) {
    this.router.navigate(['/maintainer/communes/edit', id]);
  }

  public onToggle(id: string) {
    this.communeService.toggleCommune(id).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }
        this.notificationService.showSuccess("Exito", response.message);
        this.loadCommunes();
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
