import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FranchiseDto } from '@app/core/models';
import { FranchiseService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-franchises',
    imports: [RouterLink, ButtonComponent, TableComponent],
    templateUrl: './franchises.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FranchisesComponent implements OnInit { 

  private franchiseService = inject(FranchiseService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

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
      { icon: 'bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi-toggle-on', actionKey: 'toggle'},
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
    this.router.navigate(['/maintainer/franchises/edit', id]);
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
