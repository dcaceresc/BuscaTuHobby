import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FranchiseService } from '../../../core/services/maintainer/franchise.service';
import { Router, RouterLink } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { FranchiseDto } from '../../../core/models/maintainer/franchise.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { FaIconService } from '../../../core/services/fa-icon.service';

@Component({
  selector: 'app-franchises',
  standalone: true,
  imports: [
    RouterLink,ButtonComponent,TableComponent
  ],
  templateUrl: './franchises.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FranchisesComponent implements OnInit { 

  private franchiseService = inject(FranchiseService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<FranchiseDto[]>([]);
  public actions: any[] = [];

  public ngOnInit(){

    this.columns = [
      { name: '#', key: 'franchiseId' },
      { name: 'Nombre', key : 'franchiseName'},
      { name: 'Acciones', key: 'isActive'}
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ]

    this.loadFranchises();
  }

  public loadFranchises(): void {
    this.franchiseService.getFranchises().subscribe({
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
    this.router.navigate(['/maintainer/franchises/update', id]);
  }

  public onToggle(id:string){
    this.franchiseService.toggleFranchise(id).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.ngOnInit();
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
