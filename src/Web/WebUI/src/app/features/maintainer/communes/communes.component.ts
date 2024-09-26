import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { Router, RouterLink } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { FaIconService } from '../../../core/services/fa-icon.service';
import { CommuneDto } from '../../../core/models/maintainer/commune.model';
import { CommuneService } from '../../../core/services/maintainer/commune.service';

@Component({
  selector: 'app-communes',
  standalone: true,
  imports: [
    CommonModule,ButtonComponent,TableComponent,RouterLink
  ],
  templateUrl: './communes.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
    this.router.navigate(['/maintainer/communes/update', id]);
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
