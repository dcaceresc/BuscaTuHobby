import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FranchiseService } from '../../../core/services/maintainer/franchise.service';
import { Router, RouterLink } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { FranchiseDto } from '../../../core/models/maintainer/franchise.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';

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

  public columns :any[] = [];
  public data = signal<FranchiseDto[]>([]);

  public ngOnInit(){

    this.columns = [
      { name: '#', key: 'franchiseId' },
      { name: 'Nombre', key : 'franchiseName'},
      { name: 'Acciones', key: 'isActive'}
    ];

    this.franchiseService.getFranchises().subscribe({
      next: (response) => {

        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.data.set(response.data);
      },
      error: (error) => {
        this.notificationService.showError('Error', error);
      }
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


}
